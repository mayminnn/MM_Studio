namespace TestOps.API.Models
{
    public class RunExecutionResponse
    {
        public Guid RunId { get; set; }

        public List<object> Results { get; set; } = new();
    }
}