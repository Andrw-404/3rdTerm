using MyNUnit;

namespace MyNUnitWebTask;
public class TestRunInfo
{
    public int RunId {  get; set; }
    public DateTime RunTime {  get; set; }
    public List<string> LoadedAssemblies {  get; set; } = new();
    public List<TestResult> Tests { get; set; } = new();
    public int PassedCount => Tests.Count(x => x.IsSuccess && !x.IsIgnored);
    public int FailedCount => Tests.Count(x => !x.IsSuccess && !x.IsIgnored);
    public int IgnoredCount => Tests.Count(x => x.IsIgnored);

}