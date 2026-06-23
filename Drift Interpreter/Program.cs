internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: drift <file.drf>");
            return;
        }

        var source = File.ReadAllText(args[0]);


        /*string code =
                """
                void greet()
                {
                    return 5;
                }
                greet();
                """;
       */
        //  Lexer
        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        foreach (var token in tokens) Console.WriteLine($"{token.Type} : {token.Value}");
        //  Parser
        var parser = new Parser(tokens);
        var program = parser.Parse();

        // Interpreter
        var interpreter = new Interpreter();
        interpreter.Run(program);
    }
}