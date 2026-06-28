internal class Program
{
    private static void Main(string[] args)
    {
        /*if (args.Length == 0)
        {
            Console.WriteLine("Usage: drift <file.drf>");
            return;
        }

        var source = File.ReadAllText(args[0]);
        */

        var code =
            """
            void greet()
            {
                const i8 x = 16;
                println(x);
            }
            greet();
            """;

        //  Lexer
        var lexer = new Lexer(code);
        var tokens = lexer.Tokenize();

        //foreach (var token in tokens) Console.WriteLine($"{token.Type} : {token.Value}");
        //  Parser
        var parser = new Parser(tokens);
        var program = parser.Parse();
        
        // Semantic Analyser
        var analyser = new SemanticAnalyser();
        analyser.Analyse(program);

        // Interpreter
        var interpreter = new Interpreter();
        interpreter.Run(program);
    }
}