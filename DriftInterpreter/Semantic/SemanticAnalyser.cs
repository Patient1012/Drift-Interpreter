public class SemanticAnalyser
{
    private void AnalyseStatement(Statement statement)
    {
        if (statement is VarDeclaration v)
        {
            Console.WriteLine($"{v.Name} : {v.Type}");
        }
    }
    public void Analyse(List<Statement> statements)
    {
        foreach (var statement in statements)
        {
            AnalyseStatement(statement);
        }
    }
}