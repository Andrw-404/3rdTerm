namespace MyNUnit;

public class TestResult
{
    public string ClassName {  get; set; }
    public string MethodName {  get; set; }
    public bool IsSuccess {  get; set; }
    public bool IsIgnored {  get; set; }
    public string IgnoreReason {  get; set; }
    public string ErrorMessage {  get; set; }
    public TimeSpan TestTime {  get; set; }
}