using TestOps.API.Models;

namespace TestOps.API.Services
{
    // Builds per-release (installer version) reports from the saved run results
    public class ReportService
    {
        // Label for results saved before installer version was captured
        public const string UnspecifiedRelease = "(unspecified)";

        private const string Passed = "Passed";
        private const string Failed = "Failed";
        private const string NotRun = "Not Run";
        private const string NotExecuted = "Not Executed";

        private readonly ResultService _resultService;
        private readonly IConfiguration _config;

        public ReportService(ResultService resultService, IConfiguration config)
        {
            _resultService = resultService;
            _config = config;
        }

        // Releases newest first, ordered by when testing on them started
        public List<ReleaseSummary> GetReleases()
        {
            return _resultService.GetAll()
                .GroupBy(r => ReleaseKey(r.InstallerVersion))
                .Select(g => new ReleaseSummary
                {
                    InstallerVersion = g.Key,
                    FirstRun = g.Min(r => r.ExecutedAt),
                    LastRun = g.Max(r => r.ExecutedAt),
                    Runs = g.Select(r => r.RunId).Distinct().Count(),
                    Passed = g.Count(r => r.Status == Passed),
                    Failed = g.Count(r => r.Status == Failed),
                    PassRate = Rate(g.Count(r => r.Status == Passed), g.Count(r => r.Status == Failed))
                })
                .OrderByDescending(x => x.FirstRun)
                .ToList();
        }

        public ReleaseReport? GetReport(string installerVersion, string? compareTo = null)
        {
            string release = ReleaseKey(installerVersion);

            var all = _resultService.GetAll();
            var results = all.Where(r => ReleaseKey(r.InstallerVersion) == release).ToList();

            if (results.Count == 0)
                return null;

            // Default comparison: the release tested just before this one
            if (compareTo == null)
            {
                compareTo = GetReleases()
                    .Where(x => x.InstallerVersion != release && x.FirstRun < results.Min(r => r.ExecutedAt))
                    .Select(x => x.InstallerVersion)
                    .FirstOrDefault();
            }

            bool hasPrevious = !string.IsNullOrEmpty(compareTo);

            var previous = hasPrevious
                ? all.Where(r => ReleaseKey(r.InstallerVersion) == ReleaseKey(compareTo)).ToList()
                : new List<TestRunResult>();

            // Automated tests from the library; only these map to TestLink IDs
            var library = ScanLibrary();

            var rows = library
                .SelectMany(t => t.TestLinkIds.Select(id => (Id: id.Trim(), Test: t)))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Select(g => BuildRow(g.Key, g.Select(x => x.Test).ToList(), results, previous, hasPrevious))
                .OrderBy(r => r.TestLinkId, NaturalComparer.Instance)
                .ToList();

            var report = new ReleaseReport
            {
                InstallerVersion = release,
                ComparedTo = hasPrevious ? ReleaseKey(compareTo) : null,
                FirstRun = results.Min(r => r.ExecutedAt),
                LastRun = results.Max(r => r.ExecutedAt),
                Runs = results.Select(r => r.RunId).Distinct().Count(),
                Projects = results
                    .Select(r => string.IsNullOrEmpty(r.ProjectName) ? "-" : $"{r.ProjectName} / {r.Version}")
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList(),

                Total = rows.Count,
                Passed = rows.Count(x => x.Status == Passed),
                Failed = rows.Count(x => x.Status == Failed),
                NotExecuted = rows.Count(x => x.Status == NotExecuted),
                Flaky = rows.Count(x => x.Flaky),
                Regressions = rows.Count(x => x.PreviousStatus == Passed && x.Status == Failed),
                Fixed = rows.Count(x => x.PreviousStatus == Failed && x.Status == Passed),

                TestCases = rows
            };

            report.PassRate = Rate(report.Passed, report.Failed);

            return report;
        }

        private static ReportTestLinkRow BuildRow(
            string testLinkId,
            List<TestCaseInfo> tests,
            List<TestRunResult> results,
            List<TestRunResult> previous,
            bool hasPrevious)
        {
            var coverage = tests
                .OrderBy(t => t.ClassName, NaturalComparer.Instance)
                .Select(t => new ReportCoverage
                {
                    ClassName = t.ClassName,
                    Status = TestStatus(ResultsFor(results, t.ClassName))
                })
                .ToList();

            return new ReportTestLinkRow
            {
                TestLinkId = testLinkId,
                Name = string.Join(" / ", tests.Select(t => t.MethodName).Distinct()),
                Status = CombinedStatus(coverage.Select(c => c.Status)),
                PreviousStatus = hasPrevious
                    ? CombinedStatus(tests.Select(t => TestStatus(ResultsFor(previous, t.ClassName))))
                    : null,
                Flaky = tests.Any(t => IsFlaky(ResultsFor(results, t.ClassName))),
                AutomatedBy = coverage
            };
        }

        private static List<TestRunResult> ResultsFor(List<TestRunResult> results, string className)
        {
            return results
                .Where(r => string.Equals(r.ClassName, className, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // One automated test: latest pass/fail wins; stopped/not-run entries never override a real verdict
        private static string TestStatus(List<TestRunResult> results)
        {
            var lastVerdict = results
                .Where(r => r.Status == Passed || r.Status == Failed)
                .OrderBy(r => r.ExecutedAt)
                .LastOrDefault();

            if (lastVerdict != null)
                return lastVerdict.Status;

            return results.Count > 0 ? NotRun : NotExecuted;
        }

        // One TestLink ID: failed if any covering test failed, passed only if all of them passed
        private static string CombinedStatus(IEnumerable<string> testStatuses)
        {
            var statuses = testStatuses.ToList();

            if (statuses.Any(s => s == Failed))
                return Failed;

            if (statuses.Count > 0 && statuses.All(s => s == Passed))
                return Passed;

            return NotExecuted;
        }

        private static bool IsFlaky(List<TestRunResult> results)
        {
            return results.Any(r => r.Status == Passed) && results.Any(r => r.Status == Failed);
        }

        private List<TestCaseInfo> ScanLibrary()
        {
            var path = _config["TestCasePath"];

            return string.IsNullOrWhiteSpace(path)
                ? new List<TestCaseInfo>()
                : TestCaseScanner.Scan(path);
        }

        private static string ReleaseKey(string? installerVersion)
        {
            return string.IsNullOrWhiteSpace(installerVersion) || installerVersion == UnspecifiedRelease
                ? UnspecifiedRelease
                : installerVersion.Trim();
        }

        private static double Rate(int passed, int failed)
        {
            return passed + failed == 0 ? 0 : Math.Round(passed * 100.0 / (passed + failed), 1);
        }

        // Sorts "tFJ-9" before "tFJ-10" and "A9" before "A10"
        private class NaturalComparer : IComparer<string>
        {
            public static readonly NaturalComparer Instance = new();

            public int Compare(string? x, string? y)
            {
                var (xText, xNumber) = Split(x ?? "");
                var (yText, yNumber) = Split(y ?? "");

                int byText = string.Compare(xText, yText, StringComparison.OrdinalIgnoreCase);

                return byText != 0 ? byText : xNumber.CompareTo(yNumber);
            }

            private static (string Text, long Number) Split(string value)
            {
                int i = value.Length;

                while (i > 0 && char.IsDigit(value[i - 1]))
                    i--;

                return i < value.Length && long.TryParse(value.Substring(i), out long n)
                    ? (value.Substring(0, i), n)
                    : (value, -1);
            }
        }
    }
}
