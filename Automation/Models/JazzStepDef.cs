namespace Automation.Models
{
    public class JazzStepDef
    {
        public string Label { get; set; } = "";
        public string Command { get; set; } = "";
        public string Parameter { get; set; } = "";
        public string PassPath { get; set; } = "";
        public string FailPath { get; set; } = "";
        public string PassBin { get; set; } = "";
        public string FailBin { get; set; } = "";
        public string Result { get; set; } = "";
    }
}