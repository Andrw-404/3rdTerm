// <copyright file="TestResult.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnit;

/// <summary>
/// Represents a data structure for storing the complete result of a single test method.
/// </summary>
public class TestResult
{
    /// <summary>
    /// Gets or sets the name of the assembly containing the test.
    /// </summary>
    public string? AssemblyName { get; set; }

    /// <summary>
    /// Gets or sets the name of the test class containing the test method.
    /// </summary>
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets the name of the test method itself.
    /// </summary>
    public string? MethodName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets a flag indicating whether the test passed successfully (true).
    /// If the test failed or was ignored, this value is false.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets a flag indicating whether the test was ignored.
    /// </summary>
    public bool IsIgnored { get; set; }

    /// <summary>
    /// Gets or sets the reason why the test was ignored.
    /// </summary>
    public string? IgnoreReason { get; set; }

    /// <summary>
    /// Gets or sets the error message or exception details if the test failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the time taken to execute the test.
    /// </summary>
    public TimeSpan TestTime { get; set; }
}