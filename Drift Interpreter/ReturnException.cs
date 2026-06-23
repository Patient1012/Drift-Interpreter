public class ReturnException : Exception
{
    public object? Value;

    public ReturnException(object? value)
    {
        Value = value;
    }
}