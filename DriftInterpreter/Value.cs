public class Value
{
    public object? Data;
    public ValueType? Type;

    public bool IsConst;
    public bool WasModified;

    public Value(object? Data, ValueType? Type, bool IsConst, bool WasModified)
    {
        this.Data = Data;
        this.Type = Type;
        this.IsConst = IsConst;
        this.WasModified = false;
    }
}