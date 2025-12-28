// <copyright file="AfterClassAttribute.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace Attributes;

/// <summary>
/// An attribute that marks the method to be executed once after completing all tests in the class.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterClassAttribute : Attribute
{
}