// <copyright file="MultithreadedVersion.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    /// <summary>
    /// Thread-safe implementation of lazy evaluation.
    /// </summary>
    /// <typeparam name="T">Type of calculated value.</typeparam>
    public class MultithreadedVersion<T> : ILazy<T>
    {
        private readonly object locker = new object();

        private volatile Func<T>? supplier;

        private T? result;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultithreadedVersion{T}"/> class.
        /// </summary>
        /// <param name="supplier">A function that calculates the value.</param>
        /// <exception cref="ArgumentNullException">If supplier is null.</exception>
        public MultithreadedVersion(Func<T> supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);

            this.supplier = supplier;
        }

        /// <summary>
        /// Returns the calculated value. Guarantees a one-time calculation.
        /// </summary>
        /// <returns>Calculated value.</returns>
        public T Get()
        {
            if (this.supplier is not null)
            {
                lock (this.locker)
                {
                    if (this.supplier is not null)
                    {
                        this.result = this.supplier();
                        this.supplier = null;
                    }
                }
            }

            return this.result;
        }
    }
}