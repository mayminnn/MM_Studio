using System.Collections.Generic;

namespace Automation.Models
{
    public class FlowSummary
    {
        public List<int> DUT { get; set; }

        public List<string> Status { get; set; }

        public List<int> HardBin { get; set; }

        public List<int> SoftBin { get; set; }

        public List<string> BinName { get; set; }

        public FlowSummary()
        {
            DUT = new List<int>();
            Status = new List<string>();
            HardBin = new List<int>();
            SoftBin = new List<int>();
            BinName = new List<string>();
        }
    }
}