using System.Text.Json;
using Ptl.Contracts.Dtos.Hardware;

public class HardwareEventStore
{
    private readonly string _file = "pending_rx_events.json";
    private readonly object _lock = new();

    public HardwareEventStore()
    {
        if (!File.Exists(_file))
        {
            File.WriteAllText(_file, "[]");
        }
    }

    public List<PtlRxEventDto> GetAll()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_file);
            return JsonSerializer.Deserialize<List<PtlRxEventDto>>(json) ?? new();
        }
    }

    public void Add(PtlRxEventDto evt)
    {
        lock (_lock)
        {
            var events = GetAll();
            events.Add(evt);
            Save(events);
        }
    }

    public void Remove(PtlRxEventDto evt)
    {
        lock (_lock)
        {
            var events = GetAll();

            events.RemoveAll(e =>
                e.Gateway == evt.Gateway &&
                e.Tag == evt.Tag &&
                e.Command == evt.Command
            );

            Save(events);
        }
    }

    private void Save(List<PtlRxEventDto> events)
    {
        File.WriteAllText(
            _file,
            JsonSerializer.Serialize(events, new JsonSerializerOptions
            {
                WriteIndented = true
            })
        );
    }
}