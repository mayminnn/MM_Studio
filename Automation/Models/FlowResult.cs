namespace Automation.Models
{
    public class FlowResult
    {
        public string Name { get; set; }

        public List<TestResult> Tests { get; set; }
            = new List<TestResult>();
    }
}