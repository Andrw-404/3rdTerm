// <copyright file="MultithreadedVersion.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    public class MultithreadedVersion<T> : ILazy<T>
    {
        private readonly object locker = new object();

        private volatile Func<T>? supplier;

        private T? result;

        public MultithreadedVersion(Func<T> supplier)
        {
            if (supplier is null)
            {
                ArgumentNullException.ThrowIfNull(supplier);
            }

            this.supplier = supplier;
        }

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