// <copyright file="SimpleVersion.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    /// <summary>
    /// Single-threaded implementation of lazy evaluation.
    /// </summary>
    /// <typeparam name="T">Type of calculated value.</typeparam>
    public class SimpleVersion<T> : ILazy<T>
    {
        private Func<T>? supplier;

        private T? result;

        private bool flag;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleVersion{T}"/> class.
        /// </summary>
        /// <param name="supplier">A function that calculates the value.</param>
        /// <exception cref="ArgumentNullException">If supplier is null.</exception>
        public SimpleVersion(Func<T> supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);

            this.supplier = supplier;
            this.flag = false;
        }

        /// <summary>
        /// Returns the calculated value. Performs the calculation on the first call,
        /// returns the previously calculated value on subsequent attempts.
        /// </summary>
        /// <returns>Calculated value.</returns>
        public T Get()
        {
            if (!this.flag)
            {
                this.result = this.supplier!();
                this.flag = true;
                this.supplier = null;
            }

            return this.result;
        }
    }
}