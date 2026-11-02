public class CallExpression : Expression
{
    public string Name;
    public List<Expression> Arguments;

    public CallExpression(string name, List<Expression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}