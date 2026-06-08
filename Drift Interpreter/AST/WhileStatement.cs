public class WhileStatement : Statement
{
    public Expression Condition;
    public List<Statement> Body;

    public WhileStatement(Expression condition, List<Statement> body)
    {
        Condition = condition;
        Body = body;
    }
}