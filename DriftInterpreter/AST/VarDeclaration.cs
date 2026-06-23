public class VarDeclaration : Statement
{
    public string Name;
    public Expression Value;
    public string Type;
    public bool IsConst;

    public VarDeclaration(string name, Expression value, string type, bool isConst)
    {
        Name = name;
        Value = value;
        Type = type;
        IsConst = isConst;
    }
}