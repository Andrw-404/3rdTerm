// <copyright file="TestRunServiceTests.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnitWebTask.Tests;

using Attributes;
using MyNUnit;

public class TestRunServiceTests
{
    private string testDirectory;
    private TestRunService testRunService;

    [Before]
    public void SetUp()
    {
        this.testRunService = new TestRunService();
        this.testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (Directory.Exists(this.testDirectory))
        {
            foreach (var file in Directory.GetFiles(this.testDirectory, "*.dll"))
            {
                File.Delete(file);
            }
        }
    }

    [Test]
    public void GetHistory_CalledWithoutHisttory_ShouldReturnEmptyList()
    {
        var history = this.testRunService.GetHistory();

        Assert.IsNotNull(history);
        Assert.AreEqual(0, history.Count);
    }

    [Test]
    public void RunTests_EmptyList_ShouldReturnTestRunWithZeroTest()
    {
        var emptyList = new List<string>();
        var result = this.testRunService.RunTests(emptyList);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Tests);
        Assert.AreEqual(0, result.Tests.Count);
    }

    [Test]
    public void GetHistory_AfterRunTests_ContainsNewRun()
    {
        var emptyList = new List<string>();
        this.testRunService.RunTests(emptyList);
        var history = this.testRunService.GetHistory();
        Assert.AreEqual(1, history.Count);
    }

    [Test]
    public void GetHistory_ShouldReturnsRunsInDescendingOrder()
    {
        this.testRunService.RunTests(new List<string>());
        this.testRunService.RunTests(new List<string>());
        this.testRunService.RunTests(new List<string>());
        var history = this.testRunService.GetHistory();

        Assert.AreEqual(3, history.Count);
        Assert.IsTrue(history[0].RunId > history[1].RunId);
        Assert.IsTrue(history[1].RunId > history[2].RunId);
    }

    [Test]
    public void GetLastRun_AfterMultipleRuns_ReturnsLastOne()
    {
        this.testRunService.RunTests(new List<string>());
        this.testRunService.RunTests(new List<string>());
        this.testRunService.RunTests(new List<string>());
        var lastRun = this.testRunService.GetLastRun();
        var history = this.testRunService.GetHistory();

        Assert.IsNotNull(lastRun);
        Assert.AreEqual(history[0].RunId, lastRun.RunId);
    }
}