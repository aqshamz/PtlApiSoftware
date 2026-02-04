using Ptl.Agent.Domain;
using Ptl.Core.Interfaces;

public class NullActionSink : IPtlActionSink
{
    public void EnqueuePendingAction(PendingDbAction action)
    {
        // Phase 5: intentionally empty (no DB yet)
        Console.WriteLine($"[PENDING] {action.ActionType}");
    }
}
