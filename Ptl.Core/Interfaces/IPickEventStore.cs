using Ptl.Contracts.Events;

public interface IPickEventStore
{
    void Append(PickConfirmedEvent evt);
    IEnumerable<PickConfirmedEvent> LoadUnprocessed();
    void MarkProcessed(string eventId);
}
