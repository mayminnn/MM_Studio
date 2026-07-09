namespace TestOps.API.Models
{
    public class SuiteTest
    {
        public string ClassName { get; set; } = "";

        public string DisplayName { get; set; } = "";

        public string ExecutionName { get; set; } = "";
    }

    public class TestSuite
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<SuiteTest> Tests { get; set; } = new();
    }
}