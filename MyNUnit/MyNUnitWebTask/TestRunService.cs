using MyNUnit;

namespace MyNUnitWebTask;

public class TestRunService
{
    private static List<TestRunInfo> allRuns = new List<TestRunInfo>();
    private static int nextRunId = 1;

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
                LoadedAssemblies = assemblyPaths.Select(Path.GetFileName).ToList() ?? new(),
                Tests = testResult.Select(x => new TestInfo
                {
                    TestName = $"{x.ClassName}.{x.MethodName}",
                    Status = x.IsIgnored ? "Ignored" : (x.IsSuccess ? "Passed" : "Failed"),
                    ExecutionTime = (long)x.TestTime.TotalMilliseconds,
                    Message = x.IsIgnored ? x.IgnoreReason ?? "" : x.ErrorMessage ?? ""
                }).ToList()
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
            catch { }
        }
    }

    public List<TestRunInfo> GetHistory()
    {
        return allRuns.OrderByDescending(x => x.RunId).ToList();
    }

    public TestRunInfo? GetLastRun()
    {
        return allRuns.OrderByDescending(x => x.RunId).FirstOrDefault();
    }
}