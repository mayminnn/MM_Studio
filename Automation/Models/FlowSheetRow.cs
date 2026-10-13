namespace Automation.Models
{
    public class FlowSheetRow
    {
        public string Label { get; set; } = "";
        public string Enable { get; set; } = "";
        public string Command { get; set; } = "";
        public string Parameter { get; set; } = "";
        public string Property { get; set; } = "";
        public string Value { get; set; } = "";
        public string TestName { get; set; } = "";
        public string TestNum { get; set; } = "";
        public double? LimitLow { get; set; }
        public double? LimitHigh { get; set; }
        public string PassPath { get; set; } = "";
        public string FailPath { get; set; } = "";
        public string PassBin { get; set; } = "";
        public string FailBin { get; set; } = "";
        public string Result { get; set; } = "";
        public string Assume { get; set; } = "";
        public string Sites { get; set; } = "";
        public string Remarks { get; set; } = "";
    }
}