public class ReturnException : Exception
{
    public object? Value;
    public SourcePosition Location;

    public ReturnException(object? value, SourcePosition location)
    {
        Value = value;
        Location = location;
    }
}
