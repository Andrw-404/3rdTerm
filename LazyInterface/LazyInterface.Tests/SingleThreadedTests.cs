// <copyright file="SingleThreadedTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface.Tests
{
    public class SingleThreadedTests
    {
        [Test]
        public void Get_ShoulCallSupplierOnlyOnceAndReturnCorrectValue_MultipleCalls()
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
        public void Get_ShoulReturnTheSameObjectReference_MultipleCalls()
        {
            var lazy = new SimpleVersion<List<string>>(() => []);

            var result1 = lazy.Get();
            var result2 = lazy.Get();
            Assert.That(result1, Is.SameAs(result2));
        }

        [Test]
        public void Constructor_ShouldThrowArgumentNullException_SupplierIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SimpleVersion<object>(null!));
        }
    }
}