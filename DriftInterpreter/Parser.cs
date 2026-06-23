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
        while (!IsAtEnd()) statements.Add(ParseStatement());
        return statements;
    }

    private Statement ParseCallStatement()
    {
        var name = Advance();

        var call = ParseCall(name.Value);

        Advance(); // ;

        return new CallStatement(call);
    }

    private VarDeclaration ParseVarDeclaration()
    {
        var keyword = Advance(); // var or const
        var isConst = keyword.Type == TokenType.Const;

        var type = Advance(); // i32
        var name = Advance(); // x

        Advance(); // =

        var valueExpression = ParseExpression();

        Advance(); // ;

        return new VarDeclaration(name.Value, valueExpression, type.Value, isConst);
    }

    private FunctionDeclaration ParseFunction()
    {
        var returnType = Advance(); // i32, void etc
        var name = Advance(); // name of the function

        Advance(); // (

        List<string> parameters = new();

        while (!Match(TokenType.RightParen))
        {
            Advance(); // parameter type

            var paramName = Advance();

            parameters.Add(paramName.Value);

            if (Match(TokenType.Comma)) Advance();
        }

        Advance(); // )

        Advance(); // {

        List<Statement> body = new();

        while (!Match(TokenType.RightBrace)) body.Add(ParseStatement());

        Advance(); // }

        return new FunctionDeclaration(name.Value, returnType.Value, parameters, body);
    }

    private ReturnStatement ParseReturn()
    {
        Advance(); // return

        Expression? value = null;

        if (!Match(TokenType.Semicolon)) value = ParseExpression();

        Advance(); // ;

        return new ReturnStatement(value);
    }

    private static readonly HashSet<TokenType> TypeTokens = new()
    {
        // All the different return types
        TokenType.I8,
        TokenType.I16,
        TokenType.I32,
        TokenType.I64,

        TokenType.F16,
        TokenType.F32,
        TokenType.F64,

        TokenType.Bool,
        TokenType.Char,
        TokenType.Str,
        TokenType.Void
    };

    private bool IsType(Token token)
    {
        return TypeTokens.Contains(token.Type);
    }

    private PrintStatement ParsePrintln()
    {
        Advance(); // println
        Advance(); // (

        var value = ParseExpression(); // contents of the print statement

        Advance(); // )
        Advance(); // ;

        return new PrintStatement(value);
    }

    private IfStatement ParseIf()
    {
        Advance(); // if
        Advance(); // (

        var condition = ParseExpression(); // condition of the statement

        Advance(); // )
        Advance(); // {

        List<Statement> thenBranch = new();
        List<(Expression, List<Statement>)> elifBranches = new();
        List<Statement>? elseBranch = null;

        while (!Match(TokenType.RightBrace)) thenBranch.Add(ParseStatement());
        Advance();

        while (Match(TokenType.Elif))
        {
            Advance(); // elif
            Advance(); // (

            var elifCondition = ParseExpression(); // condition of the statement

            Advance(); // )
            Advance(); // {

            List<Statement> body = new();

            while (!Match(TokenType.RightBrace)) body.Add(ParseStatement());
            Advance(); // }
            elifBranches.Add((elifCondition, body));
        }

        if (Match(TokenType.Else))
        {
            Advance(); // else
            Advance(); // {

            elseBranch = new List<Statement>();

            while (!Match(TokenType.RightBrace)) elseBranch.Add(ParseStatement());
            Advance(); // }
        }

        Console.WriteLine($"Leaving ParseIf at {_position}, token={_tokens[_position].Type}");
        return new IfStatement(condition, thenBranch, elifBranches, elseBranch);
    }

    private WhileStatement ParseWhile()
    {
        Advance(); // while

        Advance(); // (

        var condition = ParseExpression(); // condition of the statement

        Advance(); // )

        Advance(); // {

        List<Statement> body = new();
        while (!Match(TokenType.RightBrace)) body.Add(ParseStatement());
        Advance(); // }
        return new WhileStatement(condition, body);
    }

    private Statement ParseAssignment()
    {
        var name = Advance();
        if (!Match(TokenType.Equals)) throw new Exception("Expected '=' in assignment");
        Advance(); // =
        var value = ParseExpression();
        if (!Match(TokenType.Semicolon)) throw new Exception("Expected ';' after assignment");
        Advance(); // ;
        return new AssignmentStatement(name.Value, value);
    }

    private Expression ParseExpression()
    {
        Console.WriteLine($"[EXPR] {_tokens[_position].Type}");
        Console.WriteLine($"ParseStatement: Position={_position}, Token={_tokens[_position].Type}");
        var left = ParsePrimary();

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
            var op = Advance().Value;
            var right = ParsePrimary();
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    private Expression ParsePrimary()
    {
        var token = Advance();

        if (token.Type == TokenType.Number) return new LiteralExpression(int.Parse(token.Value));
        if (token.Type == TokenType.String) return new LiteralExpression(token.Value);
        if (token.Type == TokenType.Identifier)
        {
            if (_position < _tokens.Count && _tokens[_position].Type == TokenType.LeftParen)
                return ParseCall(token.Value);
            return new VariableExpression(token.Value);
        }
        if (token.Type == TokenType.Identifier)
        {
            string name = token.Value;

            if (Match(TokenType.LeftSquareBrace))
            {
                Advance(); // [

                Expression x = ParseExpression();

                Advance(); // ,

                Expression y = ParseExpression();

                Advance(); // ]

                return new ArrayAccess(name, x, y);
            }

            return new VariableExpression(name);
        }

        throw new Exception("Unexpected token in expression");
    }

    private CallExpression ParseCall(string name)
    {
        Advance(); // consume '('

        List<Expression> arguments = new();

        if (!Match(TokenType.RightParen))
            do
            {
                arguments.Add(ParseExpression());
            } while (Match(TokenType.Comma) && Advance() != null);

        Advance(); // consume ')'

        return new CallExpression(name, arguments);
    }

    private Statement ParseStatement()
    {
        if (_tokens[_position].Type == TokenType.Comma)
            throw new Exception("Parser desync: comma reached statement level");
        
        Console.WriteLine($"STATEMENT: {_tokens[_position].Type} at {_position}");
        Console.WriteLine($"ParseStatement: Position={_position}, Token={_tokens[_position].Type}");
        
        if (_position + 1 < _tokens.Count && _tokens[_position].Type == TokenType.Identifier &&
            _tokens[_position + 1].Type == TokenType.Equals)
            return ParseAssignment();
        
        if (_position + 1 < _tokens.Count && _tokens[_position].Type == TokenType.Identifier &&
            _tokens[_position + 1].Type == TokenType.LeftParen)
            return ParseCallStatement();
        
        if (Match(TokenType.Var)) return ParseVarDeclaration();
        
        if (Match(TokenType.Var))
        {
            Token type = Advance();
            Token name = Advance();

            Advance(); // [

            Token width = Advance();
            Advance(); // ,

            Token height = Advance();
            Advance(); // ]

            Advance(); // ;

            return new ArrayDeclaration(name.Value, int.Parse(width.Value), int.Parse(height.Value));
        }

        if (Match(TokenType.Const)) return ParseVarDeclaration();
        if (Match(TokenType.Println)) return ParsePrintln();
        if (Match(TokenType.If)) return ParseIf();
        if (Match(TokenType.While)) return ParseWhile();
        if (Match(TokenType.For)) return ParseWhile();
        if (Match(TokenType.Return)) return ParseReturn();
        if (IsType(_tokens[_position]) && _tokens[_position + 1].Type == TokenType.Identifier &&
            _tokens[_position + 2].Type == TokenType.LeftParen)
            return ParseFunction();
        throw new Exception($"Unknown statement: {_tokens[_position].Type}");
    }

    private Token Advance()
    {
        if (_position >= _tokens.Count) throw new Exception("Parser advanced past EOF");
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