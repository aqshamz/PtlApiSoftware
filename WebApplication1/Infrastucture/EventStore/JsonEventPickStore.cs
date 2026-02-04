using Ptl.Contracts.Events;
using System.Text.Json;

public class JsonPickEventStore : IPickEventStore
{
    private readonly string _file = "pick-events.json";
    private readonly object _lock = new();

    public void Append(PickConfirmedEvent evt)
    {
        lock (_lock)
        {
            var list = LoadAll();
            list.Add(evt);
            Save(list);
        }
    }

    public IEnumerable<PickConfirmedEvent> LoadUnprocessed()
        => LoadAll().Where(e => !e.Processed);

    public void MarkProcessed(string eventId)
    {
        lock (_lock)
        {
            var list = LoadAll();
            var evt = list.First(e => e.EventId == eventId);
            if (evt == null) return;

            evt.Processed = true;
            Save(list);
        }
    }

    private List<PickConfirmedEvent> LoadAll()
    {
        if (!File.Exists(_file))
            return new();

        var json = File.ReadAllText(_file);
        return JsonSerializer.Deserialize<List<PickConfirmedEvent>>(json)
               ?? new();
    }

    private void Save(List<PickConfirmedEvent> list)
        => File.WriteAllText(_file, JsonSerializer.Serialize(list));
}
