// <copyright file="SingleThreadedTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface.Tests
{
    public class SingleThreadedTests
    {
        [Test]
        public void Get_MultipleCalls_ShoulCallSupplierOnlyOnceAndReturnCorrectValue()
        {
            int callCount = 0;
            Func<string> supplier = () =>
            {
                callCount++;
                return "abc";
            };
            var lazy = new SimpleVersion<string>(supplier);

            lazy.Get();
            lazy.Get();
            lazy.Get();

            var result = lazy.Get();

            Assert.That(callCount, Is.EqualTo(1));
            Assert.That(result, Is.EqualTo("abc"));
        }

        [Test]
        public void Get_MultipleCalls_ShoulReturnTheSameObjectReference()
        {
            var lazy = new SimpleVersion<List<string>>(() => []);

            var result1 = lazy.Get();
            var result2 = lazy.Get();
            Assert.That(result1, Is.SameAs(result2));
        }

        [Test]
        public void GetFromSingleThreadedVersion_SupplierReturnsNull_ShouldReturnNull()
        {
            int callCount = 0;
            var lazy = new SimpleVersion<object>(() =>
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
        public void ConstructorSingleThreadVersion_SupplierIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleVersion<object>(null!));
        }
    }
}