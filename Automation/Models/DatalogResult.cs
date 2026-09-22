using System.Collections.Generic;
using System.Linq;

namespace Automation.Models
{
    public class DatalogResult
    {
        public string FlowName { get; set; }

        public List<DatalogStep> Steps { get; set; }

        public List<TestLimitResult> TestLimits { get; set; }

        public FlowSummary Summary { get; set; }

        public DatalogResult()
        {
            Steps = new List<DatalogStep>();
            TestLimits = new List<TestLimitResult>();
            Summary = new FlowSummary();
        }

        public TestLimitResult GetTest(string name)
        {
            return TestLimits.FirstOrDefault(x => x.Name == name);
        }
    }
}