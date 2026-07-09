namespace TestOps.API.Models
{
    public class DashboardSummary
    {
        public int TestCases { get; set; }

        public int TestSuites { get; set; }

        public int TotalTestsInSuites { get; set; }

        public string LastSuite { get; set; } = "";
    }
}