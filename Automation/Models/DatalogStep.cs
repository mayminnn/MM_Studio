namespace Automation.Models
{
    public class DatalogStep
    {
        public string Description { get; set; } = "";

        public string TestTime { get; set; }

        public TestLimitResult TestLimit { get; set; }
    }
}