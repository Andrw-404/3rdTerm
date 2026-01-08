// <copyright file="MyThreadPool.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyThreadPoolTask;

/// <summary>
/// A thread pool with a fixed number of threads to complete tasks.
/// </summary>
public class MyThreadPool
{
    private readonly Thread[] workers;

    private readonly Queue<Action> taskQueue = new();

    private readonly object queueLock = new();

    private readonly CancellationTokenSource cancellationTokenSource = new();

    private int isShutdown = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadCount">Number of threads in the pool.</param>
    /// <exception cref="ArgumentOutOfRangeException">It is thrown if threadCount is less than or equal to 0.
    /// </exception>
    public MyThreadPool(int threadCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(threadCount, 0);

        this.workers = new Thread[threadCount];
        for (int i = 0; i < threadCount; ++i)
        {
            this.workers[i] = new Thread(this.WorkerLoop)
            {
                IsBackground = true,
            };

            this.workers[i].Start();
        }
    }

    /// <summary>
    /// Adds a task to the thread pool for execution.
    /// </summary>
    /// <typeparam name="TResult">Task result type.</typeparam>
    /// <param name="func">Function to execute.</param>
    /// <returns>Task object that allows tracking execution and getting result.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when pool is shutdown.
    /// </exception>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {

        ArgumentNullException.ThrowIfNull(func);

        var task = new MyTask<TResult>(func, this);
        this.EnqueueAction(task.Execute);
        return task;
    }

    /// <summary>
    /// Stops all threads after completing current tasks.
    /// </summary>
    public void Shutdown()
    {
        if (Interlocked.CompareExchange(ref this.isShutdown, 1, 0) != 0)
        {
            return;
        }

        this.cancellationTokenSource.Cancel();
        lock (this.queueLock)
        {
            Monitor.PulseAll(this.queueLock);
        }

        foreach (var workerThread in this.workers)
        {
            workerThread.Join();
        }

        this.cancellationTokenSource.Dispose();
    }

    /// <summary>
    /// Method for adding action to execution queue.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    /// <exception cref="InvalidOperationException">Thrown when pool is shutdown.
    /// </exception>
    internal void EnqueueAction(Action action)
    {
        lock (this.queueLock)
        {
            if (this.isShutdown != 0)
            {
                throw new InvalidOperationException("Пул остановлен");
            }

            this.taskQueue.Enqueue(action);

            Monitor.Pulse(this.queueLock);
        }
    }

    private void WorkerLoop()
    {
        while (!this.cancellationTokenSource.IsCancellationRequested)
        {
            Action? taskAction = null;
            lock (this.queueLock)
            {
                while (this.taskQueue.Count == 0)
                {
                    if (this.cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }

                    Monitor.Wait(this.queueLock);

                    if (this.cancellationTokenSource.IsCancellationRequested && this.taskQueue.Count == 0)
                    {
                        return;
                    }
                }

                taskAction = this.taskQueue.Dequeue();
            }

            taskAction?.Invoke();
        }
    }

    private class MyTask<TResult> : IMyTask<TResult>
    {
        private readonly MyThreadPool pool;
        private readonly Lock taskLock = new();
        private readonly ManualResetEventSlim resultReady = new(false);
        private readonly List<(Action Execute, Action OnShutDown, Action<AggregateException> OnParentException)> followUpActions = new();

        private Func<TResult>? func;
        private TResult? result;
        private AggregateException? exception;
        private volatile bool isCompleted;

        public MyTask(Func<TResult> func, MyThreadPool pool)
        {
            ArgumentNullException.ThrowIfNull(func);
            ArgumentNullException.ThrowIfNull(pool);
            this.func = func;
            this.pool = pool;
        }

        public bool IsCompleted => this.isCompleted;

        public TResult Result
        {
            get
            {
                this.resultReady.Wait();

                if (this.exception is not null)
                {
                    throw this.exception;
                }

                return this.result!;
            }
        }

        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> next)
        {
            ArgumentNullException.ThrowIfNull(next);

            lock (this.taskLock)
            {

                var nextTask = new MyTask<TNewResult>(
                    () =>
                {
                    var currentResult = this.Result;
                    return next(currentResult);
                },
                    this.pool);

                if (this.IsCompleted)
                {
                    if (this.exception is not null)
                    {
                        nextTask.SetParentException(this.exception);
                    }
                    else
                    {
                        this.pool.EnqueueAction(nextTask.Execute);
                    }
                }
                else
                {
                    this.followUpActions.Add((nextTask.Execute, nextTask.SetShutdownException, nextTask.SetParentException));
                }

                return nextTask;
            }
        }

        internal void SetShutdownException()
        {
            lock (this.taskLock)
            {
                if (this.isCompleted)
                {
                    return;
                }

                this.exception = new AggregateException(new InvalidOperationException("Пул остановлен"));
                this.isCompleted = true;
                this.resultReady.Set();
            }
        }

        internal void SetParentException(AggregateException parentException)
        {
            lock (this.taskLock)
            {
                if (this.isCompleted)
                {
                    return;
                }

                this.exception = parentException;
                this.isCompleted = true;
                this.resultReady.Set();
            }
        }

        internal void Execute()
        {
            try
            {
                if (this.func is not null)
                {
                    this.result = this.func();
                }
            }
            catch (Exception exception)
            {
                this.exception = new AggregateException(exception);
            }
            finally
            {
                lock (this.taskLock)
                {
                    this.func = null;
                    this.isCompleted = true;

                    this.resultReady.Set();

                    foreach (var (execute, onShutdown, onParentException) in this.followUpActions)
                    {
                        if (this.exception is not null)
                        {
                            onParentException(this.exception);
                        }
                        else
                        {
                            try
                            {
                                this.pool.EnqueueAction(execute);
                            }
                            catch (InvalidOperationException)
                            {
                                onShutdown();
                            }
                        }
                    }

                    this.followUpActions.Clear();
                }
            }
        }
    }
}