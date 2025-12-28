// <copyright file="Index.cshtml.cs" company="Kalinin Andrew">
// Copyright (c) Kalinin Andrew. All rights reserved.
// </copyright>

namespace MyNUnitWebTask.Pages;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// <summary>
/// Page model for the Index page that handles file uploads and test execution.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> logger;
    private readonly TestRunService testRunService;

    /// <summary>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging.</param>
    public IndexModel(ILogger<IndexModel> logger)
    {
        this.logger = logger;
        this.testRunService = new TestRunService();
    }

    /// <summary>
    /// Gets or sets the list of uploaded DLL file names.
    /// </summary>
    public List<string> UploadedFiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the information about the last test run.
    /// </summary>
    public TestRunInfo? LastRun { get; set; }

    /// <summary>
    /// Gets or sets the history of all test runs.
    /// </summary>
    public List<TestRunInfo> History { get; set; } = new();

    /// <summary>
    /// Handles GET requests to the Index page. Loads uploaded files and test run history.
    /// </summary>
    public void OnGet()
    {
        this.LoadUploadedFiles();
        this.LastRun = this.testRunService.GetLastRun();
        this.History = this.testRunService.GetHistory();
    }

    /// <summary>
    /// Handles POST requests for file upload. Validates and saves the uploaded DLL file.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <returns>The page result after processing the upload.</returns>
    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            this.ModelState.AddModelError(string.Empty, "Выберите файл");
            this.LoadUploadedFiles();
            return this.Page();
        }

        if (!file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            this.ModelState.AddModelError(string.Empty, "Требуется .dll файл");
            this.LoadUploadedFiles();
            return this.Page();
        }

        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, file.FileName);
        using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        this.LoadUploadedFiles();
        return this.Page();
    }

    /// <summary>
    /// Handles POST requests to run tests on all uploaded DLL files.
    /// </summary>
    /// <returns>A JSON result containing test run results and history.</returns>
    public IActionResult OnPostRunTests()
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        var files = Directory.GetFiles(uploadDir, "*.dll");

        var result = this.testRunService.RunTests(files.ToList());
        var history = this.testRunService.GetHistory();

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
                    testTime = t.TestTime.TotalMilliseconds,
                }),
            },
            history = history.Select(h => new
            {
                runId = h.RunId,
                passedCount = h.PassedCount,
                failedCount = h.FailedCount,
                ignoredCount = h.IgnoredCount,
            }),
        });
    }

    /// <summary>
    /// Handles POST requests to delete an uploaded file.
    /// </summary>
    /// <param name="fileName">The name of the file to delete.</param>
    /// <returns>A redirect to the Index page.</returns>
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

        return this.RedirectToPage();
    }

    /// <summary>
    /// Loads the list of uploaded DLL files from the uploads directory.
    /// </summary>
    private void LoadUploadedFiles()
    {
        var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (Directory.Exists(uploadDir))
        {
            this.UploadedFiles = Directory.GetFiles(uploadDir).Select(Path.GetFileName).Where(f => !string.IsNullOrEmpty(f)).Cast<string>().ToList();
        }
    }
}