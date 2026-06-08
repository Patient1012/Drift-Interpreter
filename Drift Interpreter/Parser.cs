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
    
    private IfStatement ParseIf()
    {
        Advance(); // if

        Advance(); // (

        Expression condition = ParseExpression();

        Advance(); // )

        Advance(); // {

        List<Statement> thenBranch = new();

        while (!Match(TokenType.RightBrace))
        {
            thenBranch.Add(ParseStatement());
        }

        Advance(); // }

        return new IfStatement(condition, thenBranch, null);
    }

    private Expression ParseExpression()
    {
        Expression left = ParsePrimary();

        while (_position < _tokens.Count &&
               (_tokens[_position].Type == TokenType.Plus ||
                _tokens[_position].Type == TokenType.Minus ||
                _tokens[_position].Type == TokenType.Less ||
                _tokens[_position].Type == TokenType.Greater ||
                _tokens[_position].Type == TokenType.LessEqual ||
                _tokens[_position].Type == TokenType.GreaterEqual ||
                _tokens[_position].Type == TokenType.EqualEqual ||
                _tokens[_position].Type == TokenType.NotEqual))
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
        
        if (token.Type == TokenType.String)
            return new LiteralExpression(token.Value);

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

        if (Match(TokenType.If))
        {
            return ParseIf();
        }

        throw new Exception($"Unknown statement: {_tokens[_position].Type}");
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