public class PrintStatement : Statement
{
    public List<Expression> Arguments;

    public PrintStatement(List<Expression> arguments)
    {
        Arguments = arguments;
    }
}
