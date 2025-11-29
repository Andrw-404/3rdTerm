// <copyright file="BeforeAttribute.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute that marks the method to be executed before each test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeAttribute : Attribute
{
}