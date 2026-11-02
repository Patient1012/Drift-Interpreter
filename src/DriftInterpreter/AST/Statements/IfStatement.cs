public class IfStatement : Statement
{
    public Expression Condition;
    public List<Statement> ThenBranch;
    public List<(Expression Condition, List<Statement> Body)> ElifBranches;
    public List<Statement>? ElseBranch;

    public IfStatement(
        Expression condition,
        List<Statement> thenBranch,
        List<(Expression Condition, List<Statement> Body)> elifBranches,
        List<Statement>? elseBranch)
    {
        Condition = condition;
        ThenBranch = thenBranch;
        ElifBranches = elifBranches;
        ElseBranch = elseBranch;
    }
}
