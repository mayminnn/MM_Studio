namespace Automation.Models
{
    public class ExpectedBin
    {
        public int HardBin { get; set; }

        public int SoftBin { get; set; }

        public ExpectedBin(int hardBin, int softBin)
        {
            HardBin = hardBin;
            SoftBin = softBin;
        }
    }
}