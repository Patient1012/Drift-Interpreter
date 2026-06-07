public class BinaryExpression : Expression
{
    public Expression Left;
    public string Operator;
    public Expression Right;

    public BinaryExpression(Expression left, string op, Expression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }
}