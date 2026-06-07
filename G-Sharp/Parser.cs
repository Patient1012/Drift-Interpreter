public class Parser
{
    private List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }
    public List<Statement> Parse()
    {
        List<Statement> statements = new();

        while (!IsAtEnd())
        {
            statements.Add(ParseStatement());
        }

        return statements;
    }
    private VarDeclaration ParseVarDeclaration()
    {
        Advance(); // var

        Token type = Advance(); // i32
        Token name = Advance(); // x

        Advance(); // =
        
        Expression value = ParseExpression();
        
        Advance(); // ;

        return new VarDeclaration(name.Value, value);
    }
    private PrintStatement ParsePrintln()
    {
        Advance(); // println

        Advance(); // (

        Expression value = ParseExpression();

        Advance(); // )

        Advance(); // ;

        return new PrintStatement(value);
    }

    private Expression ParseExpression()
    {
        Expression left = ParsePrimary();

        while (_position < _tokens.Count &&
               (_tokens[_position].Type == TokenType.Plus ||
                _tokens[_position].Type == TokenType.Minus))
        {
            string op = Advance().Value;

            Expression right = ParsePrimary();

            left = new BinaryExpression(left, op, right);
        }

        return left;
    }
    private Expression ParsePrimary()
    {
        Token token = Advance();

        if (token.Type == TokenType.Number)
            return new LiteralExpression(int.Parse(token.Value));

        if (token.Type == TokenType.Identifier)
            return new VariableExpression(token.Value);

        throw new Exception("Unexpected token in expression");
    }
    private Statement ParseStatement()
    {
        Console.WriteLine($"Position={_position}, Token={_tokens[_position].Type}");
        if (Match(TokenType.Var))
        {
            return ParseVarDeclaration();
        }
        
        if (Match(TokenType.Println))
        {
            return ParsePrintln();
        }

        throw new Exception("Unknown statement");
    }
    private Token Advance()
    {
        return _tokens[_position++];
    }

    private bool Match(TokenType type)
    {
        return _tokens[_position].Type == type;
    }

    private bool IsAtEnd()
    {
        return _tokens[_position].Type == TokenType.EOF;
    }
}