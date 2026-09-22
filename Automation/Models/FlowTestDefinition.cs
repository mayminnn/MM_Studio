namespace Automation.Models
{
    public class FlowTestDefinition
    {
        public string TestName { get; set; }

        public int TestNum { get; set; }

        public double? LimitLow { get; set; }

        public double? LimitHigh { get; set; }

        public string? PassPath { get; set; }

        public string? FailPath { get; set; }

        public int? PassBin { get; set; }

        public int? FailBin { get; set; }

        public string? Result { get; set; }
    }
}