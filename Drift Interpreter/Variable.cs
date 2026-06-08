public class Variable
{
    public object Value;
    public bool IsConst;

    public Variable(object value, bool isConst)
    {
        Value = value;
        IsConst = isConst;
    }
}