// <copyright file="MyThreadPool.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyThreadPoolTask
{
    /// <summary>
    /// A thread pool with a fixed number of threads to complete tasks.
    /// </summary>
    public class MyThreadPool
    {
        private readonly Thread[] workers;

        private readonly Queue<Action> taskQueue = new();

        private readonly object queueLock = new();

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private volatile bool isShutdown = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
        /// </summary>
        /// <param name="threadCount">Number of threads in the pool.</param>
        /// <exception cref="ArgumentOutOfRangeException">It is thrown if threadCount is less than or equal to 0.
        /// </exception>
        public MyThreadPool(int threadCount)
        {
            if (threadCount <= 0)
            {
                throw new ArgumentOutOfRangeException($"Количество потоков {threadCount} должно быть больше нуля");
            }

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
            if (this.isShutdown)
            {
                throw new InvalidOperationException("Пул остановлен");
            }

            ArgumentNullException.ThrowIfNull(func, nameof(func));

            var task = new MyTask<TResult>(func, this);
            this.EnqueueAction(task.Execute);
            return task;
        }

        /// <summary>
        /// Stops all threads after completing current tasks.
        /// </summary>
        public void Shutdown()
        {
            if (this.cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            this.isShutdown = true;
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
                if (this.isShutdown)
                {
                    throw new InvalidOperationException("Пул останолен");
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
            private readonly object taskLock = new();
            private readonly ManualResetEventSlim resultReady = new(false);
            private readonly List<Action> followUpActions = new();

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

                if (this.pool.cancellationTokenSource.IsCancellationRequested)
                {
                    throw new InvalidOperationException();
                }

                lock (this.taskLock)
                {
                    if (this.pool.cancellationTokenSource.IsCancellationRequested && this.pool.isShutdown)
                    {
                        throw new InvalidOperationException();
                    }

                    var nextTask = new MyTask<TNewResult>(
                        () =>
                    {
                        var currentResult = this.Result;
                        return next(currentResult);
                    },
                        this.pool);

                    if (this.IsCompleted)
                    {
                        try
                        {
                            this.pool.EnqueueAction(nextTask.Execute);
                        }
                        catch (InvalidOperationException)
                        {
                            throw new InvalidOperationException("Пул остановлен");
                        }
                    }
                    else
                    {
                        this.followUpActions.Add(nextTask.Execute);
                    }

                    return nextTask;
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

                        foreach (var action in this.followUpActions)
                        {
                            try
                            {
                                this.pool.EnqueueAction(action);
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }

                        this.followUpActions.Clear();
                    }
                }
            }
        }
    }
}