namespace Automation.Models
{
    public class FinalSummary
    {
        public List<string> Status { get; set; }
            = new List<string>();

        public List<int> HardBin { get; set; }
            = new List<int>();

        public List<int> SoftBin { get; set; }
            = new List<int>();

        public List<string> BinName { get; set; }
            = new List<string>();
    }
}