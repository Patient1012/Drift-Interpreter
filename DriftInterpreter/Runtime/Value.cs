public class Value
{
    public object Data { get; set; }
    public ValueType Type { get; set; }

    public bool IsConst { get; set; }
    public bool WasModified { get; set; }

    public Value(object data, ValueType type, bool isConst, bool wasModified = false)
    {
        Data = data;
        Type = type;
        IsConst = isConst;
        WasModified = wasModified;
    }
}