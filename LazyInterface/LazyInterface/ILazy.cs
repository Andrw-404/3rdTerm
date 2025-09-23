// <copyright file="ILazy.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    public interface ILazy<T>
    {
        T Get();
    }
}