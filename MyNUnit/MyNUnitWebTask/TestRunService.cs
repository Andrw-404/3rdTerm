// <copyright file="TestRunService.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnitWebTask;

using MyNUnit;

/// <summary>
/// Service for managing and executing test runs.
/// </summary>
public class TestRunService
{
    private static List<TestRunInfo> allRuns = new List<TestRunInfo>();
    private static int nextRunId = 1;

    /// <summary>
    /// Runs tests from the specified assembly paths and stores the results.
    /// </summary>
    /// <param name="assemblyPaths">List of paths to the DLL assemblies containing tests.</param>
    /// <returns>A <see cref="TestRunInfo"/> object containing the test execution results.</returns>
    public TestRunInfo RunTests(List<string> assemblyPaths)
    {
        var testRuner = new TestRunner();

        var tmpDir = Path.Combine(Path.GetTempPath(), $"MyNUnit_{Guid.NewGuid()}");
        Directory.CreateDirectory(tmpDir);

        try
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                if (File.Exists(assemblyPath))
                {
                    var fileName = Path.GetFileName(assemblyPath);
                    File.Copy(assemblyPath, Path.Combine(tmpDir, fileName), true);
                }
            }

            var testResult = testRuner.RunTest(tmpDir);
            var runInfo = new TestRunInfo
            {
                RunId = nextRunId++,
                RunTime = DateTime.Now,
                LoadedAssemblies = assemblyPaths.Select(Path.GetFileName).OfType<string>().ToList(),
                Tests = testResult.ToList(),
            };

            allRuns.Add(runInfo);
            return runInfo;
        }
        finally
        {
            try
            {
                Directory.Delete(tmpDir, true);
            }
            catch
            {
            }
        }
    }

    /// <summary>
    /// Gets the history of all test runs ordered by run ID in descending order.
    /// </summary>
    /// <returns>A list of <see cref="TestRunInfo"/> objects representing all test runs.</returns>
    public List<TestRunInfo> GetHistory()
    {
        return allRuns.OrderByDescending(x => x.RunId).ToList();
    }

    /// <summary>
    /// Gets the most recent test run.
    /// </summary>
    /// <returns>The latest <see cref="TestRunInfo"/> object, or null if no runs have been executed.</returns>
    public TestRunInfo? GetLastRun()
    {
        return allRuns.OrderByDescending(x => x.RunId).FirstOrDefault();
    }
}