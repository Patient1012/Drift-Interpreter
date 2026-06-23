public class ReturnStatement : Statement
{
    public Expression? Value;

    public ReturnStatement(Expression? value)
    {
        Value = value;
    }
}