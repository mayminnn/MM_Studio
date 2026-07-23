using TestOps.API.Models;

namespace TestOps.API.Services;

public class ExecutionTracker
{
    private readonly Dictionary<Guid, ExecutionStatus> _runs = new();

    public void Add(ExecutionStatus status)
    {
        _runs[status.RunId] = status;
    }

    public ExecutionStatus? Get(Guid runId)
    {
        _runs.TryGetValue(runId, out var status);

        return status;
    }

    public void Update(ExecutionStatus status)
    {
        _runs[status.RunId] = status;
    }
}