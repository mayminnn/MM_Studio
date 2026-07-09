using System.Linq;
using TestOps.API.Models;

namespace TestOps.API.Services
{
    public class DashboardService
    {
        private readonly SuiteService _suiteService;
        private readonly IConfiguration _config;

        public DashboardService(
            SuiteService suiteService,
            IConfiguration config)
        {
            _suiteService = suiteService;
            _config = config;
        }

        public DashboardSummary GetSummary()
        {
            var suites = _suiteService.GetAll();

            var path = _config["TestCasePath"];

            var testCases = string.IsNullOrWhiteSpace(path)
                ? new List<TestCaseInfo>()
                : TestCaseScanner.Scan(path);

            return new DashboardSummary
            {
                TestCases = testCases.Count,
                TestSuites = suites.Count,
                TotalTestsInSuites = suites.Sum(x => x.Tests.Count),
                LastSuite = suites.LastOrDefault()?.Name ?? "-"
            };
        }
    }
}