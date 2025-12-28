// <copyright file="TestRunInfo.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnitWebTask;

using MyNUnit;

/// <summary
/// Contains information about a test run including results and statistics.
/// </summary>
public class TestRunInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the test run.
    /// </summary>
    public int RunId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the test run was executed.
    /// </summary>
    public DateTime RunTime { get; set; }

    /// <summary>
    /// Gets or sets the list of assembly names that were loaded and executed during the test run.
    /// </summary>
    public List<string> LoadedAssemblies { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of test results from the test run.
    /// </summary>
    public List<TestResult> Tests { get; set; } = new();

    /// <summary>
    /// Gets the count of tests that passed (not ignored).
    /// </summary>
    public int PassedCount => this.Tests.Count(x => x.IsSuccess && !x.IsIgnored);

    /// <summary>
    /// Gets the count of tests that failed (not ignored).
    /// </summary>
    public int FailedCount => this.Tests.Count(x => !x.IsSuccess && !x.IsIgnored);

    /// <summary>
    /// Gets the count of tests that were ignored.
    /// </summary>
    public int IgnoredCount => this.Tests.Count(x => x.IsIgnored);
}