// <copyright file="CommonTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface.Tests;

public class CommonTests
{
    public static IEnumerable<ILazy<object>> LazyImplementations()
    {
        yield return new SimpleVersion<object>(() => new object());
        yield return new MultithreadedVersion<object>(() => new object());
    }

    [TestCaseSource(nameof(LazyImplementations))]
    public void Get_MultipleCalls_ShoulCallSupplierOnlyOnce(ILazy<object> lazy)
    {
        int callCount = 0;
        Func<object> supplier = () =>
        {
            callCount++;
            return new object();
        };
        lazy = lazy is SimpleVersion<object> ? new SimpleVersion<object>(supplier) : new MultithreadedVersion<object>(supplier);

        lazy.Get();
        lazy.Get();
        lazy.Get();

        var result = lazy.Get();

        Assert.That(callCount, Is.EqualTo(1));
    }

    [TestCaseSource(nameof(LazyImplementations))]
    public void Get_MultipleCalls_ShoulReturnTheSameObjectReference(ILazy<object> lazy)
    {
        lazy = lazy is SimpleVersion<object> ? new SimpleVersion<object>(() => new object()) : new MultithreadedVersion<object>(() => new object());

        var result1 = lazy.Get();
        var result2 = lazy.Get();
        Assert.That(result1, Is.SameAs(result2));
    }

    [TestCaseSource(nameof(LazyImplementations))]
    public void Get_SupplierReturnsNull_ShouldReturnNull(ILazy<object> lazy)
    {
        int callCount = 0;
        lazy = lazy is SimpleVersion<object> ? new SimpleVersion<object>(() =>
        {
            callCount++;
            return null!;
        }) : new MultithreadedVersion<object>(() =>
        {
            callCount++;
            return null!;
        });

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.That(callCount, Is.EqualTo(1));
        Assert.That(result1, Is.Null);
        Assert.That(result2, Is.Null);
    }

    [TestCaseSource(nameof(LazyImplementations))]
    public void Constructor_SupplierIsNull_ShouldThrowArgumentNullException(ILazy<object> lazy)
    {
        Assert.Throws<ArgumentNullException>(() => new SimpleVersion<object>(null!));
        Assert.Throws<ArgumentNullException>(() => new MultithreadedVersion<object>(null!));
    }
}