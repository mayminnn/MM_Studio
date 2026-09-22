namespace Automation.Models
{
    public class TestResult
    {
        public string Name { get; set; }

        public string Flow { get; set; }

        public double LowLimit { get; set; }

        public double HighLimit { get; set; }

        public string Unit { get; set; }

        public List<double> SiteValues { get; set; }
            = new List<double>();
    }
}