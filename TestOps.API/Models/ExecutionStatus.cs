namespace TestOps.API.Models;

public class ExecutionStatus
{
    public Guid RunId { get; set; }

    public string SuiteName { get; set; } = "";

    public int TotalTests { get; set; }

    public int CompletedTests { get; set; }

    public string CurrentTest { get; set; } = "";

    public bool Finished { get; set; }

    public List<TestRunResult> Results { get; set; } = new();
}