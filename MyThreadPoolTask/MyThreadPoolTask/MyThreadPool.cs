using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyThreadPoolTask
{
    public class MyThreadPool
    {
        private readonly Thread[] workers;

        private readonly Queue<Action> taskQueue = new();

        private readonly object queueLock = new();

        private readonly CancellationTokenSource cancellationTokenSource = new();


        public MyThreadPool(int threadCount)
        {
            if (threadCount <= 0)
            {
                throw new ArgumentOutOfRangeException($"Количество потоков {threadCount} должно быть больше нуля");
            }

            this.workers = new Thread[threadCount];
            for (int i = 0; i < threadCount; ++i)
            {
                this.workers[i] = new Thread(WorkerLoop)
                {
                    IsBackground = true
                };

                workers[i].Start();
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

        private void EnqueueAction(Action action)
        {
            lock (this.queueLock)
            {
                if (this.cancellationTokenSource.IsCancellationRequested)
                {
                    throw new InvalidOperationException();
                }

               this.taskQueue.Enqueue(action);

                Monitor.Pulse(this.queueLock);
            }
        }

        private class MyTask<TResult> : IMyTask<TResult>
        {
            private readonly MyThreadPool pool;
            private Func<TResult> func;
            private readonly object taskLock = new();
            private readonly ManualResetEventSlim ResultReady = new(false);

            private TResult? result;
            private AggregateException? exception;
            private volatile bool isCompleted;

            private readonly List<Action> followUpActions = new();

            public MyTask(Func<TResult> func, MyThreadPool pool)
            {
                ArgumentNullException.ThrowIfNull(func);
                this.func = func;
                this.pool = pool;
            }
            public bool IsCompleted => this.isCompleted;

            public TResult Result
            {
                get
                {
                    this.ResultReady.Wait();

                    if (exception is not null)
                    {
                        throw this.exception;
                    }
                    return this.result;
                }
            }

            public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> next)
            {
                ArgumentNullException.ThrowIfNull(next);

                if (this.pool.cancellationTokenSource.IsCancellationRequested)
                {
                    throw new InvalidOperationException();
                }

                var nextTask = new MyTask<TNewResult>(() =>
                {
                    var sourceResult = this.Result;
                    return next(sourceResult);
                }, pool);

                lock (this.taskLock)
                {
                    if (this.pool.cancellationTokenSource.IsCancellationRequested)
                    {
                        throw new InvalidOperationException();
                    }

                    if (this.IsCompleted)
                    {
                        this.pool.EnqueueAction(nextTask.Execute);
                    }
                    else
                    {
                        this.followUpActions.Add(nextTask.Execute);
                    }
                }

                return nextTask;
            }

            internal void Execute()
            {
                try
                {
                    if (this.func != null)
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

                        this.ResultReady.Set();

                        foreach (var action in this.followUpActions)
                        {
                            try
                            {
                                this.pool.EnqueueAction(action);
                            }
                            catch(InvalidOperationException)
                            {

                            }
                        }
                        this.followUpActions.Clear();
                    }
                }
            }
        };
    }
}