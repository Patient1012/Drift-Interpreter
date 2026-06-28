public class Variable
{
    public string Type;
    public object Value;
    public bool IsConst;
    public bool WasModified;

    public Variable(string type, object value, bool isConst)
    {
        Type = type;
        Value = value;
        IsConst = isConst;
        WasModified = false;
    }
}