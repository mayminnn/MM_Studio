using System.Collections.Generic;

namespace TestOps.API.Models
{
    // public class RunSuiteRequest
    // {
    //     public string? SuiteName { get; set; }
    //     public List<string> Tests { get; set; } = new();
    // }

    public class RunSuiteRequest
    {
        public string SuiteName { get; set; } = "";

        // Project under test; blank falls back to the TestCases defaults
        public string ProjectName { get; set; } = "";

        public string Version { get; set; } = "";

        public string InstallerVersion { get; set; } = "";

        public List<SuiteTest> Tests { get; set; } = new();
    }
}