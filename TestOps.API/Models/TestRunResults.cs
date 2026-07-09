namespace TestOps.API.Models
{
    public class TestRunResult
    {
        public Guid Id { get; set; }
        public Guid RunId { get; set; }
        public string SuiteName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Status { get; set; } = "";
        public string Output { get; set; } = "";
        public string Error { get; set; } = "";
        public DateTime ExecutedAt { get; set; }
    }
}