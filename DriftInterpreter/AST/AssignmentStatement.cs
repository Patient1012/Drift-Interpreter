public class AssignmentStatement : Statement
{
    public string Name;
    public Expression Value;

    public AssignmentStatement(string name, Expression value)
    {
        Name = name;
        Value = value;
    }
}