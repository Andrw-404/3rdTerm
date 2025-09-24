// <copyright file="MultithreadedTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface.Tests
{
    public class MultithreadedTests
    {
        [Test]
        public void Get_AvailabilityOfRaces_SupplierShouldInitializeOnlyOnce()
        {
            int callCount = 0;
            var lazy = new MultithreadedVersion<object>(() =>
            {
                Interlocked.Increment(ref callCount);
                Thread.Sleep(150);
                return new object();
            });

            int numOfThreads = 10;
            var threads = new Thread[numOfThreads];
            var results = new object[numOfThreads];

            for (int i = 0; i < numOfThreads; ++i)
            {
                int index = i;
                threads[i] = new Thread(() =>
                {
                    results[index] = lazy.Get();
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            Assert.That(callCount, Is.EqualTo(1));
            var firstResult = results[0];
            Assert.That(results.All(res => ReferenceEquals(res, firstResult)), Is.True);
        }

        [Test]
        public void GetFromMultithreadedVersion_SupplierReturnsNull_ShouldReturnNull()
        {
            int callCount = 0;
            var lazy = new MultithreadedVersion<object>(() =>
            {
                callCount++;
                return null;
            });

            var result1 = lazy.Get();
            var result2 = lazy.Get();

            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(result1, Is.Null);
            Assert.That(result2, Is.Null);
        }

        [Test]
        public void ConstructorMultithreadVersion_SupplierIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MultithreadedVersion<object>(null!));
        }
    }
}