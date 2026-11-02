public class VarDeclaration : Statement {
    public string Name { get; }
    public Expression Value { get; }
    public string Type { get; }
    public bool IsConst { get; }

    public VarDeclaration(string name, Expression value, string type, bool isConst) {
        Name = name;
        Value = value;
        Type = type;
        IsConst = isConst;
    }
};
