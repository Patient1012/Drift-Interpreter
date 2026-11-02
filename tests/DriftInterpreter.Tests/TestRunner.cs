using Xunit;

internal static class TestRunner
{
    public static string Run(string source)
    {
        var output = new StringWriter();
        var previous = Console.Out;

        Console.SetOut(output);

        try
        {
            Execute(source);
        }
        finally
        {
            Console.SetOut(previous);
        }

        return output.ToString().Replace("\r\n", "\n");
    }

    public static TException CaptureException<TException>(string source)
        where TException : Exception
    {
        var output = new StringWriter();
        var previous = Console.Out;

        Console.SetOut(output);

        try
        {
            return Assert.Throws<TException>(() => Execute(source));
        }
        finally
        {
            Console.SetOut(previous);
        }
    }

    private static void Execute(string source)
    {
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser(tokens);
        var program = parser.Parse();

        var interpreter = new Interpreter();
        interpreter.Run(program);
    }
}
