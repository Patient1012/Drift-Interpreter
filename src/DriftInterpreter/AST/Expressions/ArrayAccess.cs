public class ArrayAccess : Expression
{
    public string Name;
    public Expression X;
    public Expression Y;

    public ArrayAccess(string name, Expression x, Expression y)
    {
        Name = name;
        X = x;
        Y = y;
    }
}