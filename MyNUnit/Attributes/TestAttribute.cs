// <copyright file="TestAttribute.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute that marks a method as a test method.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestAttribute : Attribute
{
    /// <summary>
    /// Gets or sets expected exception.
    /// </summary>
    public Type? Expected { get; set; }

    /// <summary>
    /// Gets or sets the reason for ignoring.
    /// </summary>
    public string? Ignore { get; set; }
}