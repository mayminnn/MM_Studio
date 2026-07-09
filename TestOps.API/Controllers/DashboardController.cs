using Microsoft.AspNetCore.Mvc;
using TestOps.API.Services;

namespace TestOps.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly ResultService _resultService;
    private readonly SuiteService _suiteService;

    public DashboardController(
        ResultService resultService,
        SuiteService suiteService)
    {
        _resultService = resultService;
        _suiteService = suiteService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var results = _resultService.GetAll();
        var suites = _suiteService.GetAll();

        var totalTests =
            suites.Sum(x => x.Tests.Count);

        var totalExecutions =
            results.Select(x => x.RunId)
                   .Distinct()
                   .Count();

        var passed =
            results.Count(x => x.Status == "Passed");

        var failed =
            results.Count(x => x.Status == "Failed");

        var passRate =
            results.Count == 0
                ? 0
                : Math.Round(
                    passed * 100.0 / results.Count,
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