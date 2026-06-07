
public class VarDeclaration : Statement
{
    public string Name;
    public Expression Value;

    public VarDeclaration(string name, Expression value)
    {
        Name = name;
        Value = value;
    }
}