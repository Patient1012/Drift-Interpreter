using System.Globalization;

public class RuntimeEnvironment
{
    private readonly Dictionary<string, Value> _values = new();

    public RuntimeEnvironment? Parent { get; }

    public RuntimeEnvironment(RuntimeEnvironment? parent = null)
    {
        Parent = parent;
    }

    public void Define(string name, Value value)
    {
        _values[name] = value;
    }

    public void Clear()
    {
        _values.Clear();
    }

    public bool TryGet(string name, out Value value)
    {
        if (_values.TryGetValue(name, out var found))
        {
            value = found;
            return true;
        }

        if (Parent != null)
            return Parent.TryGet(name, out value);

        value = null!;
        return false;
    }
}
