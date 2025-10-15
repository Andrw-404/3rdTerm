// <copyright file="ThreadPoolTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyThreadPoolTask.Tests
{
    using System.Collections.Concurrent;

    public class ThreadPoolTests
    {
        private MyThreadPool? threadPool;

        [TearDown]
        public void TearDown()
        {
            this.threadPool?.Shutdown();
        }

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
    }
}