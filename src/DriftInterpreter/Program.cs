internal static class Program
{
    internal static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("Usage: drift <file.drf>");
            return 1;
        }

        string source;
        try
        {
            source = File.ReadAllText(args[0]);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"Could not read '{args[0]}': {ex.Message}");
            return 1;
        }

        try
        {
            //  Lexer
            var lexer = new Lexer(source);
            var tokens = lexer.Tokenize();

            //  Parser
            var parser = new Parser(tokens);
            var program = parser.Parse();

            // Semantic Analyser
            var analyser = new SemanticAnalyser();
            analyser.Analyse(program);

            // Interpreter
            var interpreter = new Interpreter();
            interpreter.Run(program);

            return 0;
        }
        catch (DriftException ex)
        {
            Console.Error.WriteLine(ex.ToDiagnosticString(args[0]));
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
