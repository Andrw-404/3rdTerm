// <copyright file="ILazy.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    /// <summary>
    /// Interface for lazy value calculation.
    /// </summary>
    /// <typeparam name="T">Type of calculated value.</typeparam>
    public interface ILazy<T>
    {
        /// <summary>
        /// Gets the calculated value. Performs the calculation on the first call,
        /// returns the previously calculated value on subsequent attempts.
        /// </summary>
        /// <returns>Calculated value of type T.</returns>
        T Get();
    }
}