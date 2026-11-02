public class UnaryExpression : Expression
{
    public string Operator;
    public Expression Operand;

    public UnaryExpression(string op, Expression operand)
    {
        Operator = op;
        Operand = operand;
    }
}
