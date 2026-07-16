namespace TestOps.API.Models
{
    public class TestCaseInfo
    {
        public string ClassName { get; set; } = "";

        public string MethodName { get; set; } = "";

        public List<string> Tags { get; set; } = new();
    }
}