using System.Collections.Generic;
using System.Linq;

namespace Automation.Models
{
    public class DatalogResult
    {
        public List<TestLimitResult> TestLimits { get; set; }

        public FlowSummary Summary { get; set; }

        public DatalogResult()
        {
            TestLimits = new List<TestLimitResult>();
            Summary = new FlowSummary();
        }
        
        public TestLimitResult oldGetTest(string name)
        {
            return TestLimits.FirstOrDefault(x => x.Name == name);
        }

        public TestLimitResult GetTest(string name)
        {
            foreach (var test in TestLimits)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Comparing [{test.Name}] with [{name}]");
            }

            return TestLimits.FirstOrDefault(x => x.Name == name);
        }
    }
}