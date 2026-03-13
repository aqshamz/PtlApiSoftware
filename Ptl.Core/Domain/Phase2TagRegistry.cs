using Ptl.Agent.Domain;

public class Phase2TagRegistry
{
    private readonly Dictionary<int, TagState> _tags = new();
    private readonly object _lock = new();

    public void Set(TagState state)
    {
        lock (_lock)
        {
            _tags[state.Tag] = state;
        }
    }

    public bool TryGet(int tag, out TagState state)
    {
        lock (_lock)
        {
            return _tags.TryGetValue(tag, out state!);
        }
    }

    public void Remove(int tag)
    {
        lock (_lock)
        {
            _tags.Remove(tag);
        }
    }

    public bool Exists(int tag)
    {
        return _tags.ContainsKey(tag);
    }
}