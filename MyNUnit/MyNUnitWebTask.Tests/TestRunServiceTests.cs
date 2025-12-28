namespace MyNUnitWebTask.Tests;

using Attributes;
using MyNUnit;

public class TestRunServiceTests
{
    private TestRunService testRunService;
    public string testDirectory;

    [Before]
    public void SetUp()
    {
        testRunService = new TestRunService(); 
        testDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

        if (Directory.Exists(testDirectory))
        {
            foreach (var file in Directory.GetFiles(testDirectory, "*.dll"))
            {
                File.Delete(file);
            }
        }
    }

    [Test]
    public void GetHistory_CalledWithoutHisttory_ShouldReturnEmptyList()
    {
        var history = testRunService.GetHistory();

        Assert.IsNotNull(history);
        Assert.AreEqual(0, history.Count);
    }

    [Test]
    public void RunTests_EmptyList_ShouldReturnTestRunWithZeroTest()
    {
        var emptyList = new List<string>();
        var result = testRunService.RunTests(emptyList);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Tests);
        Assert.AreEqual(0, result.Tests.Count);
    }

    [Test]
    public void GetHistory_AfterRunTests_ContainsNewRun()
    {
        var emptyList = new List<string>();
        testRunService.RunTests(emptyList);
        var history = testRunService.GetHistory();
        Assert.AreEqual(1, history.Count);
    }

    [Test]
    public void GetHistory_ShouldReturnsRunsInDescendingOrder()
    {
        testRunService.RunTests(new List<string>());
        testRunService.RunTests(new List<string>());
        testRunService.RunTests(new List<string>());
        var history = testRunService.GetHistory();

        Assert.AreEqual(3, history.Count);
        Assert.IsTrue(history[0].RunId > history[1].RunId);
        Assert.IsTrue(history[1].RunId > history[2].RunId);
    }

    [Test]
    public void GetLastRun_AfterMultipleRuns_ReturnsLastOne()
    {
        testRunService.RunTests(new List<string>());
        testRunService.RunTests(new List<string>());
        testRunService.RunTests(new List<string>());
        var lastRun = testRunService.GetLastRun();
        var history = testRunService.GetHistory();

        Assert.IsNotNull(lastRun);
        Assert.AreEqual(history[0].RunId, lastRun.RunId);
    }
}