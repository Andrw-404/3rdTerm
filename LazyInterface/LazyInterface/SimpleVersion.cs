// <copyright file="SimpleVersion.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    public class SimpleVersion<T> : ILazy<T>
    {
        private Func<T>? supplier;

        private T? result;

        private bool flag;

        public SimpleVersion(Func<T> supplier)
        {
            ArgumentNullException.ThrowIfNull(supplier);

            this.supplier = supplier;
            this.flag = false;
        }

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