// <copyright file="ILazy.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace LazyInterface
{
    internal interface ILazy<T>
    {
        T Get();
    }
}