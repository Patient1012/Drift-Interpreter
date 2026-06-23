public class ArrayAssignment : Statement
{
    public ArrayAccess Target;
    public Expression Value;

    public ArrayAssignment(ArrayAccess target, Expression value)
    {
        Target = target;
        Value = value;
    }
}