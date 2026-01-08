// <copyright file="ThreadPoolTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyThreadPoolTask.Tests;

using System.Collections.Concurrent;

public class ThreadPoolTests
{
    private MyThreadPool? threadPool;

    [TearDown]
    public void TearDown() => this.threadPool?.Shutdown();

    [Test]
    public void Submit_SinglTask_ReturnCorrectResult()
    {
        this.threadPool = new MyThreadPool(4);
        var task = this.threadPool.Submit(() => 42 * 2);
        Assert.That(task.Result, Is.EqualTo(84));
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void Submit_MultiplyTask_ReturnCorrectResult()
    {
        this.threadPool = new MyThreadPool(5);
        var tasks = new List<IMyTask<int>>();
        for (int i = 0; i < 10; ++i)
        {
            int local = i;
            tasks.Add(this.threadPool.Submit(() => local * 2));
        }

        for (int i = 0; i < 10; ++i)
        {
            Assert.That(tasks[i].Result, Is.EqualTo(i * 2));
            Assert.That(tasks[i].IsCompleted, Is.True);
        }
    }

    [Test]
    public void Submit_AfterShutdown_ShouldThrowsException()
    {
        this.threadPool = new MyThreadPool(2);
        this.threadPool.Shutdown();
        Assert.Throws<InvalidOperationException>(() => this.threadPool.Submit(() => 2313));
    }

    [Test]
    public void Submit_TaskThrowsException_ResultThrowsAggregateException()
    {
        this.threadPool = new MyThreadPool(2);
        var task = this.threadPool.Submit<int>(() => throw new InvalidOperationException());
        var exception = Assert.Throws<AggregateException>(() => { var x = task.Result; });
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void ContinueWith_ThreeTasks_ShouldReturnCorrectResult()
    {
        this.threadPool = new MyThreadPool(4);
        var firstTask = this.threadPool.Submit(() => 10);
        var secondTask = firstTask.ContinueWith(result => result * 4);
        var thirdTask = secondTask.ContinueWith(result => result.ToString());
        var finalResult = thirdTask.Result;
        Assert.That(finalResult, Is.EqualTo("40"));
        Assert.That(firstTask.IsCompleted, Is.True);
        Assert.That(secondTask.IsCompleted, Is.True);
        Assert.That(thirdTask.IsCompleted, Is.True);
    }

    [Test]
    public void ThreadPool_NumberOfThreads_ShouldUsetheSpecifiedAmount()
    {
        const int threadCount = 5;
        this.threadPool = new MyThreadPool(threadCount);
        var threadId = new ConcurrentDictionary<int, bool>();
        var startSignal = new ManualResetEventSlim(false);
        var tasks = new List<IMyTask<bool>>();

        for (int i = 0; i < threadCount; ++i)
        {
            var task = this.threadPool.Submit(() =>
            {
                threadId.TryAdd(Thread.CurrentThread.ManagedThreadId, true);
                startSignal.Wait();
                return true;
            });
            tasks.Add(task);
        }

        Thread.Sleep(700);

        Assert.That(threadId.Count, Is.EqualTo(threadCount));

        startSignal.Set();
    }

    [Test]
    public void ContinueWith_MultipleContinuations_AllShouldExecute()
    {
        this.threadPool = new MyThreadPool(4);
        var baseTask = this.threadPool.Submit(() => 10);

        var continuation1 = baseTask.ContinueWith(x => x + 1);
        var continuation2 = baseTask.ContinueWith(x => x + 2);
        var continuation3 = baseTask.ContinueWith(x => x * 2);

        Assert.That(continuation1.Result, Is.EqualTo(11));
        Assert.That(continuation2.Result, Is.EqualTo(12));
        Assert.That(continuation3.Result, Is.EqualTo(20));
    }

    [Test]
    public void ContinueWith_OnCompletedTask_ShouldStillWork()
    {
        this.threadPool = new MyThreadPool(2);
        var task = this.threadPool.Submit(() => 5);
        var result = task.Result;

        Assert.That(task.IsCompleted, Is.True);

        var continuation = task.ContinueWith(x => x * 10);
        Assert.That(continuation.Result, Is.EqualTo(50));
    }

    [Test]
    public void Result_BeforeCompletion_ShouldBlockUntilComplete()
    {
        this.threadPool = new MyThreadPool(2);
        var startSignal = new ManualResetEventSlim(false);
        var task = this.threadPool.Submit(() =>
        {
            startSignal.Wait();
            return 4;
        });

        Assert.That(task.IsCompleted, Is.False);

        var resultTask = Task.Run(() => task.Result);

        Thread.Sleep(100);
        Assert.That(resultTask.IsCompleted, Is.False);

        startSignal.Set();
        Assert.That(resultTask.Result, Is.EqualTo(4));
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void ContinueWith_AfterShutdown_ShouldThrowException()
    {
        this.threadPool = new MyThreadPool(2);
        var task = this.threadPool.Submit(() => 10);
        var result = task.Result;

        this.threadPool.Shutdown();

        Assert.Throws<InvalidOperationException>(() => task.ContinueWith(x => x * 2));
        this.threadPool = null;
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Constructor_InvalidThreadCount_ShouldThrowException(int count)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MyThreadPool(count));
    }

    [Test]
    public void ContinueWith_WhenShutdownDuringExecution_ShouldThrowExceptionInResult()
    {
        this.threadPool = new MyThreadPool(1);
        var blockSignal = new ManualResetEventSlim(false);
        var taskStarted = new ManualResetEventSlim(false);

        var baseTask = this.threadPool.Submit(() =>
        {
            taskStarted.Set();
            blockSignal.Wait();
            return 4;
        });

        taskStarted.Wait();

        var continuation = baseTask.ContinueWith(x => x * 2);
        var shutdownThread = new Thread(() => this.threadPool!.Shutdown());
        shutdownThread.Start();

        Thread.Sleep(100);
        blockSignal.Set();

        shutdownThread.Join(TimeSpan.FromSeconds(2));
        var exception = Assert.Throws<AggregateException>(() => _ = continuation.Result);
        Assert.That(exception!.InnerException, Is.InstanceOf<InvalidOperationException>());

        this.threadPool = null;
    }

    [Test]
    public void RaceCondition_SubmitAndShutdownConcurrently_ShouldHandleGracefully()
    {
        this.threadPool = new MyThreadPool(4);
        var exceptions = new ConcurrentBag<Exception>();
        var successfulTasks = new ConcurrentBag<IMyTask<int>>();
        var shutdownStarted = new ManualResetEventSlim(false);
        var submitThreadsReady = new CountdownEvent(10);

        var submitThreads = new List<Thread>();
        for (int i = 0; i < 10; ++i)
        {
            int taskId = i;
            var thread = new Thread(() =>
            {
                submitThreadsReady.Signal();
                shutdownStarted.Wait();
                try
                {
                    var task = this.threadPool!.Submit(() => taskId);
                    successfulTasks.Add(task);
                }
                catch (InvalidOperationException ex)
                {
                    exceptions.Add(ex);
                }
            });
            submitThreads.Add(thread);
            thread.Start();
        }

        submitThreadsReady.Wait();
        shutdownStarted.Set();

        var shutdownThread = new Thread(() =>
        {
            Thread.Sleep(5);
            this.threadPool!.Shutdown();
        });
        shutdownThread.Start();

        foreach (var thread in submitThreads)
        {
            thread.Join();
        }

        shutdownThread.Join();

        Assert.That(successfulTasks.Count + exceptions.Count, Is.EqualTo(10));

        this.threadPool = null;
    }

    [Test]
    public void RaceCondition_MultipleContinueWithDuringShutdown_ShouldHandleCorrectly()
    {
        this.threadPool = new MyThreadPool(2);
        var blockSignal = new ManualResetEventSlim(false);
        var baseTask = this.threadPool.Submit(() =>
        {
            blockSignal.Wait();
            return 10;
        });

        var exceptions = new ConcurrentBag<Exception>();
        var continuations = new ConcurrentBag<IMyTask<int>>();
        var continueThreadsReady = new CountdownEvent(5);
        var shutdownSignal = new ManualResetEventSlim(false);

        var continueThreads = new List<Thread>();
        for (int i = 0; i < 5; ++i)
        {
            int multiplier = i + 1;
            var thread = new Thread(() =>
            {
                continueThreadsReady.Signal();
                shutdownSignal.Wait();
                try
                {
                    var cont = baseTask.ContinueWith(x => x * multiplier);
                    continuations.Add(cont);
                }
                catch (InvalidOperationException ex)
                {
                    exceptions.Add(ex);
                }
            });
            continueThreads.Add(thread);
            thread.Start();
        }

        continueThreadsReady.Wait();
        shutdownSignal.Set();

        var shutdownThread = new Thread(() =>
        {
            Thread.Sleep(10);
            this.threadPool!.Shutdown();
        });
        shutdownThread.Start();

        foreach (var thread in continueThreads)
        {
            thread.Join();
        }

        blockSignal.Set();
        shutdownThread.Join();

        Assert.That(continuations.Count + exceptions.Count, Is.EqualTo(5));

        this.threadPool = null;
    }

    [Test]
    public void RaceCondition_MultipleShutdownCalls_ShouldNotCrash()
    {
        this.threadPool = new MyThreadPool(4);
        var tasks = new List<IMyTask<int>>();
        for (int i = 0; i < 5; ++i)
        {
            int local = i;
            tasks.Add(this.threadPool.Submit(() =>
            {
                Thread.Sleep(50);
                return local;
            }));
        }

        var shutdownThreads = new List<Thread>();
        for (int i = 0; i < 5; ++i)
        {
            var thread = new Thread(() => this.threadPool!.Shutdown());
            shutdownThreads.Add(thread);
            thread.Start();
        }

        foreach (var thread in shutdownThreads)
        {
            thread.Join();
        }

        Assert.Pass();
        this.threadPool = null;
    }

    [Test]
    public void RaceCondition_ResultAccessDuringShutdown_ShouldNotDeadlock()
    {
        this.threadPool = new MyThreadPool(2);
        var blockSignal = new ManualResetEventSlim(false);
        var task = this.threadPool.Submit(() =>
        {
            blockSignal.Wait();
            return 42;
        });

        var resultThread = new Thread(() =>
        {
            Thread.Sleep(50);
            try
            {
                var result = task.Result;
            }
            catch
            {
            }
        });
        resultThread.Start();

        var shutdownThread = new Thread(() =>
        {
            Thread.Sleep(100);
            this.threadPool!.Shutdown();
        });
        shutdownThread.Start();

        Thread.Sleep(150);
        blockSignal.Set();

        bool resultFinished = resultThread.Join(TimeSpan.FromSeconds(2));
        bool shutdownFinished = shutdownThread.Join(TimeSpan.FromSeconds(2));

        Assert.That(resultFinished, Is.True);
        Assert.That(shutdownFinished, Is.True);

        this.threadPool = null;
    }

    [Test]
    public void RaceCondition_SubmitManyTasksConcurrently_AllShouldExecute()
    {
        this.threadPool = new MyThreadPool(4);
        var tasks = new ConcurrentBag<IMyTask<int>>();
        var submitThreads = new List<Thread>();
        var startSignal = new ManualResetEventSlim(false);

        for (int i = 0; i < 20; ++i)
        {
            int taskId = i;
            var thread = new Thread(() =>
            {
                startSignal.Wait();
                var task = this.threadPool!.Submit(() =>
                {
                    Thread.Sleep(10);
                    return taskId * 2;
                });
                tasks.Add(task);
            });
            submitThreads.Add(thread);
            thread.Start();
        }

        startSignal.Set();

        foreach (var thread in submitThreads)
        {
            thread.Join();
        }

        Assert.That(tasks.Count, Is.EqualTo(20));

        var results = new HashSet<int>();
        foreach (var task in tasks)
        {
            results.Add(task.Result);
        }

        Assert.That(results.Count, Is.EqualTo(20));
    }
}