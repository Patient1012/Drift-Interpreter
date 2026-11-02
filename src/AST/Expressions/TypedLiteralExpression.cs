public class TypedLiteralExpression : Expression
{
    public object Value;
    public ValueType Type;

    public TypedLiteralExpression(object value, ValueType type)
    {
        Value = value;
        Type = type;
    }
}