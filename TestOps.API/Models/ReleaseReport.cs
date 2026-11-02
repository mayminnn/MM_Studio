namespace TestOps.API.Models
{
    // One entry per installer version that has results
    public class ReleaseSummary
    {
        public string InstallerVersion { get; set; } = "";
        public DateTime FirstRun { get; set; }
        public DateTime LastRun { get; set; }
        public int Runs { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public double PassRate { get; set; }
    }

    // Release readiness by TestLink ID. Run-level detail lives on the Executions page.
    public class ReleaseReport
    {
        public string InstallerVersion { get; set; } = "";

        // Release this one is compared against for regressions/fixes; null if none
        public string? ComparedTo { get; set; }

        public DateTime FirstRun { get; set; }
        public DateTime LastRun { get; set; }
        public List<string> Projects { get; set; } = new();
        public int Runs { get; set; }

        // Counts are per TestLink ID
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int NotExecuted { get; set; }
        public int Flaky { get; set; }
        public int Regressions { get; set; }
        public int Fixed { get; set; }
        public double PassRate { get; set; }

        public List<ReportTestLinkRow> TestCases { get; set; } = new();
    }

    public class ReportTestLinkRow
    {
        public string TestLinkId { get; set; } = "";

        // Names of the automated tests covering this ID
        public string Name { get; set; } = "";

        // "Passed" (every covering test passed) | "Failed" (any failed) | "Not Executed" (otherwise)
        public string Status { get; set; } = "";
        public string? PreviousStatus { get; set; }

        // At least one covering test both passed and failed on this release
        public bool Flaky { get; set; }

        public List<ReportCoverage> AutomatedBy { get; set; } = new();
    }

    // An automated test (class) covering a TestLink ID, with its own latest status on this release
    public class ReportCoverage
    {
        public string ClassName { get; set; } = "";

        // "Passed" | "Failed" | "Not Run" | "Not Executed"
        public string Status { get; set; } = "";
    }
}
