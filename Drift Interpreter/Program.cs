class Program
{
    static void Main(string[] args)
    {

        string code = 
                """
                void greet()
                {
                    println("hello");
                }
                greet();
                """;
       
        //  Lexer
        Lexer lexer = new Lexer(code);
        List<Token> tokens = lexer.Tokenize();
        
        foreach (var token in tokens)
        {
            Console.WriteLine($"{token.Type} : {token.Value}");
        }
        //  Parser
        Parser parser = new Parser(tokens);
        List<Statement> program = parser.Parse();

        // Interpreter
        Interpreter interpreter = new Interpreter();
        interpreter.Run(program);
    }
    
}