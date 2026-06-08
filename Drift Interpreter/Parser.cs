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

        Token keyword = Advance(); // var or const
        bool isConst = keyword.Type == TokenType.Const;

        Token type = Advance(); // i32
        Token name = Advance(); // x

        Advance(); // =

        Expression valueExpression = ParseExpression();

        Advance(); // ;

        return new VarDeclaration(name.Value, valueExpression, type.Value, isConst);
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
        List<(Expression, List<Statement>)> elifBranches = new();
        List<Statement>? elseBranch = null;

        while (!Match(TokenType.RightBrace))
        {
            thenBranch.Add(ParseStatement());
        }

        Advance();
        
        while (Match(TokenType.Elif))
        {
            Advance(); // elif
            Advance(); // (

            Expression elifCondition = ParseExpression();

            Advance(); // )
            Advance(); // {

            List<Statement> body = new();

            while (!Match(TokenType.RightBrace))
            {
                body.Add(ParseStatement());
            }
            Advance(); // }
            elifBranches.Add((elifCondition, body));
        }
        if (Match(TokenType.Else))
        {
            Advance(); // else
            Advance(); // {

            elseBranch = new List<Statement>();

            while (!Match(TokenType.RightBrace))
            {
                elseBranch.Add(ParseStatement());
            }

            Advance(); // }
        }
        Console.WriteLine($"Leaving ParseIf at {_position}, token={_tokens[_position].Type}");
        return new IfStatement(condition, thenBranch, elifBranches, elseBranch);
    }
    private WhileStatement ParseWhile()
    {
        Advance(); // while

        Advance(); // (

        Expression condition = ParseExpression();

        Advance(); // )

        Advance(); // {

        List<Statement> body = new();

        while (!Match(TokenType.RightBrace))
        {
            body.Add(ParseStatement());
        }

        Advance(); // }

        return new WhileStatement(condition, body);
    }
    private Statement ParseAssignment()
    {
        Token name = Advance(); // identifier

        Advance(); // =

        Expression value = ParseExpression();

        Advance(); // ;

        return new AssignmentStatement(name.Value, value);
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
        if (Match(TokenType.Identifier))
        {
            return ParseAssignment();
        }
        if (Match(TokenType.Var))
        {
            return ParseVarDeclaration();
        }

        if (Match(TokenType.Const))
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
        if (Match(TokenType.While))
        {
            return ParseWhile();
        }
        if (Match(TokenType.For))
        {
            return ParseWhile(); //ParseFor();
        }
        throw new Exception($"Unknown statement: {_tokens[_position].Type}");
    }
    private Token Advance()
    {
        if (_position >= _tokens.Count)
            throw new Exception("Parser advanced past EOF");

        return _tokens[_position++];
    }

    private bool Match(TokenType type)
    {
        return _tokens[_position].Type == type;
    }

    private bool IsAtEnd()
    {
        return _position >= _tokens.Count || _tokens[_position].Type == TokenType.EOF;
    }
}