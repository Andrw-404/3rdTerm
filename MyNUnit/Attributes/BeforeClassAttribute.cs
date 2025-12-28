// <copyright file="BeforeClassAttribute.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute that marks the method that must be executed once before starting all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeClassAttribute : Attribute
{
}