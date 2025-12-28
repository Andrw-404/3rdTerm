namespace MyNUnitWebTask.Pages;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly TestRunService _testRunService;

    public List<string> UploadedFiles { get; set; } = new();
    public TestRunInfo? LastRun { get; set; }
    public List<TestRunInfo> History { get; set; } = new();
    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
        _testRunService = new TestRunService();
    }

    public void OnGet()
    {
        LoadUploadedFiles();
        LastRun = _testRunService.GetLastRun();
        History = _testRunService.GetHistory();
    }

    private void LoadUploadedFiles()
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (Directory.Exists(uploadDir))
        {
            UploadedFiles = Directory.GetFiles(uploadDir).Select(Path.GetFileName).Where(f => !string.IsNullOrEmpty(f)).ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Выберите файл");
            LoadUploadedFiles();
            return Page();
        }

        if (!file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, "Требуется .dll файл");
            LoadUploadedFiles();
            return Page();
        }

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, file.FileName);
        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        LoadUploadedFiles();
        return Page();
    }

    public IActionResult OnPostRunTests()
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        var files = Directory.GetFiles(uploadDir, "*.dll");

        var result = _testRunService.RunTests(files.ToList());
        var history = _testRunService.GetHistory();

        return new JsonResult(new
        {
            success = true,
            lastRun = new
            {
                runId = result.RunId,
                passedCount = result.PassedCount,
                failedCount = result.FailedCount,
                ignoredCount = result.IgnoredCount,
                tests = result.Tests.Select(t => new
                {
                    assemblyName = t.AssemblyName,
                    className = t.ClassName,
                    methodName = t.MethodName,
                    isSuccess = t.IsSuccess,
                    isIgnored = t.IsIgnored,
                    ignoreReason = t.IgnoreReason,
                    errorMessage = t.ErrorMessage,
                    testTime = t.TestTime.TotalMilliseconds
                })
            },
            history = history.Select(h => new
            {
                runId = h.RunId,
                passedCount = h.PassedCount,
                failedCount = h.FailedCount,
                ignoredCount = h.IgnoredCount
            })
        });
    }
        
    public IActionResult OnPostDeleteFile(string fileName)
    {
        if (!string.IsNullOrEmpty(fileName))
        {
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadDir, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        return RedirectToPage();
    }
}