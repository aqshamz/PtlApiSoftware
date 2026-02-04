using Ptl.Agent.Domain;

public interface IPtlActionSink
{
    void EnqueuePendingAction(PendingDbAction action);
}
