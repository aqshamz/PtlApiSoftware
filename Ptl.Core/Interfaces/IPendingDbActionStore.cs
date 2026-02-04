using Ptl.Agent.Domain;

namespace Ptl.Core.Interfaces
{
    public interface IPendingDbActionStore
    {
        void Enqueue(PendingDbAction action);
    }
}
