using System.Collections.Generic;

namespace Automation.Models
{
    public class TestLimitResult
    {
        public string Name { get; set; }

        public double LowLimit { get; set; }

        public double HighLimit { get; set; }

        public string Unit { get; set; }

        public List<double> SiteValues { get; set; }

        public TestLimitResult()
        {
            SiteValues = new List<double>();
        }
    }
}