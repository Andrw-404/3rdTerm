namespace MyNUnitWebTask;

public class TestInfo
{
    public string TestName { get; set; } = string.Empty;
    public string Status {  get; set; } = string.Empty;
    public long ExecutionTime { get; set; }
    public string Message { get; set; } = string.Empty; 
}

public class TestRunInfo
{
    public int RunId {  get; set; }
    public DateTime RunTime {  get; set; }
    public List<string> LoadedAssemblies {  get; set; } = new List<string>();
    public List<TestInfo> Tests { get; set; } = new List<TestInfo>();
    public int PassedCount => Tests.Count(x => x.Status == "Passed");
    public int FailedCount => Tests.Count(x => x.Status == "Failed");
    public int IgnoredCount => Tests.Count(x => x.Status == "Ignored");

}