// <copyright file="Assert.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnit;

using System.Runtime.CompilerServices;

/// <summary>
/// Provides a set of static methods for verifying conditions in tests.
/// </summary>
public static class Assert
{
    /// <summary>
    /// Verifies that two values are equal.
    /// </summary>
    /// <typeparam name="T">The type of values to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="real">The actual value.</param>
    /// <exception cref="Exception">Thrown when values are not equal.</exception>
    public static void AreEqual<T>(T expected, T real)
    {
        if (!Equals(expected, real))
        {
            throw new Exception($"Ожидалось: {expected}, получилось {real}");
        }
    }

    /// <summary>
    /// Verifies that a condition is true.
    /// </summary>
    /// <param name="condition">The condition to verify.</param>
    /// <exception cref="Exception">Thrown when condition is false.</exception>
    public static void IsTrue(bool condition)
    {
        if (!condition)
        {
            throw new Exception("Условие должно быть истинным");
        }
    }

    /// <summary>
    /// Verifies that an object is not null.
    /// </summary>
    /// <param name="obj">The object to verify.</param>
    /// <param name="expression">The expression being verified (auto-captured by compiler).</param>
    /// <exception cref="Exception">Thrown when object is null.</exception>
    public static void IsNotNull(object? obj, [CallerArgumentExpression(nameof(obj))] string? expression = null)
    {
        if (obj == null)
        {
            throw new Exception($"{expression} ожидался не null");
        }
    }

    /// <summary>
    /// Verifies that an object is null.
    /// </summary>
    /// <param name="obj">The object to verify.</param>
    /// <param name="expression">The expression being verified (auto-captured by compiler).</param>
    /// <exception cref="Exception">Thrown when object is not null.</exception>
    public static void IsNull(object? obj, [CallerArgumentExpression(nameof(obj))] string? expression = null)
    {
        if (obj != null)
        {
            throw new Exception($"{expression} ожидался null");
        }
    }
}