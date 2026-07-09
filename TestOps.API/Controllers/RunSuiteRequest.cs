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

        public List<SuiteTest> Tests { get; set; } = new();
    }
}