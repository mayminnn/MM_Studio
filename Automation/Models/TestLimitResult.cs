using System.Collections.Generic;

namespace Automation.Models
{
    public class TestLimitResult
    {
        public string Name { get; set; } = "";

        // Flow the limit ran in, as the datalog prints it: "Default", or
        // "Default>ATSPEED_SCAN" for a called sub-flow. Empty in older captures
        public string FlowPath { get; set; } = "";

        public double LowLimit { get; set; }

        public double HighLimit { get; set; }

        public string Unit { get; set; } = "";

        // One value per site, in site order. NaN: the site didn't run this test
        // (e.g. it had already jumped away or binned out)
        public List<double> SiteValues { get; set; }

        public TestLimitResult()
        {
            SiteValues = new List<double>();
        }
    }
}