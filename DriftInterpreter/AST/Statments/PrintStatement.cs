public class PrintStatement : Statement
{
    public Expression Value;

    public PrintStatement(Expression value)
    {
        Value = value;
    }
}