public class VariableExpression : Expression
{
    public string Name;

    public VariableExpression(string name)
    {
        Name = name;
    }
}