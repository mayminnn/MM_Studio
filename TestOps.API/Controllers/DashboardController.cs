using Microsoft.AspNetCore.Mvc;
using TestOps.API.Models;
using TestOps.API.Services;

namespace TestOps.API.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private const string Passed = "Passed";
    private const string Failed = "Failed";

    private static readonly int[] AllowedDays = { 7, 14, 30, 90 };

    private readonly ResultService _resultService;
    private readonly SuiteService _suiteService;
    private readonly ReportService _reportService;
    private readonly IConfiguration _config;

    public DashboardController(
        ResultService resultService,
        SuiteService suiteService,
        ReportService reportService,
        IConfiguration config)
    {
        _resultService = resultService;
        _suiteService = suiteService;
        _reportService = reportService;
        _config = config;
    }

    // GET api/dashboard?days=14 - everything is scoped to the last N days,
    // with deltas against the N days before that
    [HttpGet]
    public IActionResult Get([FromQuery] int days = 14)
    {
        if (!AllowedDays.Contains(days))
            return BadRequest($"days must be one of {string.Join(", ", AllowedDays)}");

        var results = _resultService.GetAll();

        DateTime periodStart = DateTime.Today.AddDays(-(days - 1));
        DateTime previousStart = periodStart.AddDays(-days);

        var current = results.Where(r => r.ExecutedAt >= periodStart).ToList();
        var previous = results.Where(r => r.ExecutedAt >= previousStart && r.ExecutedAt < periodStart).ToList();

        return Ok(new
        {
            days,
            current = PeriodStats(current),
            previous = PeriodStats(previous),
            coverage = Coverage(),
            totalSuites = _suiteService.GetAll().Count,
            trend = Trend(current, periodStart, days),
            topFailures = TopFailures(current),
            recentRuns = RecentRuns(current),
            latestRelease = LatestRelease()
        });
    }

    private static object PeriodStats(List<TestRunResult> results)
    {
        int passed = results.Count(r => r.Status == Passed);
        int failed = results.Count(r => r.Status == Failed);

        return new
        {
            passed,
            failed,
            notRun = results.Count - passed - failed,
            executions = passed + failed,
            runs = results.Select(r => r.RunId).Distinct().Count(),

            // null when nothing ran, so the UI shows "-" instead of a misleading 0%
            passRate = passed + failed == 0 ? (double?)null : Math.Round(passed * 100.0 / (passed + failed), 1)
        };
    }

    // One entry per day in the period, including days with no runs
    private static object Trend(List<TestRunResult> results, DateTime periodStart, int days)
    {
        var byDay = results.GroupBy(r => r.ExecutedAt.Date).ToDictionary(g => g.Key, g => g.ToList());

        return Enumerable.Range(0, days)
            .Select(i => periodStart.AddDays(i))
            .Select(day =>
            {
                byDay.TryGetValue(day, out var dayResults);
                dayResults ??= new List<TestRunResult>();

                int passed = dayResults.Count(r => r.Status == Passed);
                int failed = dayResults.Count(r => r.Status == Failed);

                return new
                {
                    date = day.ToString("yyyy-MM-dd"),
                    passed,
                    failed,
                    notRun = dayResults.Count - passed - failed
                };
            })
            .ToList();
    }

    private static object TopFailures(List<TestRunResult> results)
    {
        return results
            .GroupBy(r => r.ClassName)
            .Select(g => new
            {
                className = g.Key,
                displayName = g.OrderBy(r => r.ExecutedAt).Last().DisplayName,
                failures = g.Count(r => r.Status == Failed),
                executions = g.Count(r => r.Status == Passed || r.Status == Failed)
            })
            .Where(x => x.failures > 0)
            .OrderByDescending(x => x.failures)
            .ThenBy(x => x.className)
            .Take(5)
            .ToList();
    }

    private static object RecentRuns(List<TestRunResult> results)
    {
        return results
            .GroupBy(r => r.RunId)
            .Select(g => new
            {
                runId = g.Key,
                suiteName = g.First().SuiteName,
                installerVersion = g.First().InstallerVersion,
                startedAt = g.Min(r => r.ExecutedAt),
                passed = g.Count(r => r.Status == Passed),
                failed = g.Count(r => r.Status == Failed),
                notRun = g.Count(r => r.Status != Passed && r.Status != Failed)
            })
            .OrderByDescending(x => x.startedAt)
            .Take(6)
            .ToList();
    }

    // Size of the automated library: test classes and the TestLink IDs they cover
    private object Coverage()
    {
        var path = _config["TestCasePath"];

        var tests = string.IsNullOrWhiteSpace(path)
            ? new List<TestCaseInfo>()
            : TestCaseScanner.Scan(path);

        return new
        {
            automatedTests = tests.Count,
            testLinkIds = tests
                .SelectMany(x => x.TestLinkIds)
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
        };
    }

    // Readiness of the most recently tested installer (all time, not period-scoped)
    private object? LatestRelease()
    {
        var latest = _reportService.GetReleases().FirstOrDefault();

        if (latest == null)
            return null;

        var report = _reportService.GetReport(latest.InstallerVersion);

        if (report == null)
            return null;

        return new
        {
            report.InstallerVersion,
            report.ComparedTo,
            report.PassRate,
            report.Total,
            report.Passed,
            report.Failed,
            report.NotExecuted,
            report.Regressions,
            report.LastRun
        };
    }
}
