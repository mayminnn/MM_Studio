using Microsoft.AspNetCore.Mvc;
using TestOps.API.Services;

namespace TestOps.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ResultService _resultService;
    private readonly SuiteService _suiteService;
    private readonly IConfiguration _config;

    public DashboardController(
        ResultService resultService,
        SuiteService suiteService,
        IConfiguration config)
    {
        _resultService = resultService;
        _suiteService = suiteService;
        _config = config;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var results = _resultService.GetAll();
        var suites = _suiteService.GetAll();

        var totalTests =
            suites.Sum(x => x.Tests.Count);

        // Unique TestLink IDs covered by all automated test classes
        var testCasePath = _config["TestCasePath"];

        var totalTestLinkIds =
            string.IsNullOrWhiteSpace(testCasePath)
                ? 0
                : TestCaseScanner.Scan(testCasePath)
                    .SelectMany(x => x.TestLinkIds)
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

        var totalExecutions =
            results.Select(x => x.RunId)
                   .Distinct()
                   .Count();

        var passed =
            results.Count(x => x.Status == "Passed");

        var failed =
            results.Count(x => x.Status == "Failed");

        var passRate =
            passed + failed == 0
                ? 0
                : Math.Round(
                    passed * 100.0 / (passed + failed),
                    1);

        var recentExecutions =
            results
            .GroupBy(x => x.RunId)
            .OrderByDescending(x => x.Max(y => y.ExecutedAt))
            .Take(5)
            .Select(g => new
            {
                RunId = g.Key,
                Suite = g.First().SuiteName,
                Time = g.Max(x => x.ExecutedAt),
                Passed = g.Count(x => x.Status == "Passed"),
                Failed = g.Count(x => x.Status == "Failed")
            });

        var topFailures =
            results
            .Where(x => x.Status == "Failed")
            .GroupBy(x => x.DisplayName)
            .OrderByDescending(x => x.Count())
            .Take(5)
            .Select(g => new
            {
                Test = g.Key,
                Count = g.Count()
            });

        return Ok(new
        {
            totalTests,
            totalTestLinkIds,
            totalSuites = suites.Count,
            totalExecutions,
            passed,
            failed,
            passRate,
            recentExecutions,
            topFailures
        });
    }
}