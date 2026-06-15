public class FunctionDeclaration : Statement
{
    public string Name;
    public string ReturnType;
    public List<string> Parameters;
    public List<Statement> Body;

    public FunctionDeclaration(string name, string returnType, List<string> parameters, List<Statement> body)
    {
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
        Body = body;
    }
}