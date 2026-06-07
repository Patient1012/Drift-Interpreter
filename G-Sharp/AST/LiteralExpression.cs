public class LiteralExpression : Expression
{
    public object Value;

    public LiteralExpression(object value)
    {
        Value = value;
    }
}