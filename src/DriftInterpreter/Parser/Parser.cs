public class Parser
{
    private readonly List<Token> _tokens;
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

    private static T AtStatement<T>(T statement, SourcePosition location) where T : Statement
    {
        statement.Location = location;
        return statement;
    }

    private static T AtExpression<T>(T expression, SourcePosition location) where T : Expression
    {
        expression.Location = location;
        return expression;
    }

    private Statement ParseCallStatement()
    {
        var name = Consume(TokenType.Identifier, "Expected a function name");
        var call = ParseCall(name);
        Consume(TokenType.Semicolon, "Expected ';' after function call");
        return AtStatement(new CallStatement(call), name.Location);
    }

    private VarDeclaration ParseVarDeclaration()
    {
        var keyword = Advance(); // var or const
        var isConst = keyword.Type == TokenType.Const;

        var type = ConsumeType();
        var name = Consume(TokenType.Identifier, "Expected a variable name");

        Consume(TokenType.Equals, "Expected '=' after variable name");
        var valueExpression = ParseExpression();
        Consume(TokenType.Semicolon, "Expected ';' after variable declaration");

        return AtStatement(
            new VarDeclaration(name.Value, valueExpression, type.Value, isConst),
            keyword.Location);
    }

    private VarDeclaration ParseTypedVarDeclaration()
    {
        var type = ConsumeType();
        var name = Consume(TokenType.Identifier, "Expected a variable name");

        Consume(TokenType.Equals, "Expected '=' after variable name");
        var valueExpression = ParseExpression();
        Consume(TokenType.Semicolon, "Expected ';' after variable declaration");

        return AtStatement(
            new VarDeclaration(name.Value, valueExpression, type.Value, false),
            type.Location);
    }

    private FunctionDeclaration ParseFunction()
    {
        var returnType = ConsumeType();
        var name = Consume(TokenType.Identifier, "Expected a function name");

        Consume(TokenType.LeftParen, "Expected '(' after function name");

        List<string> parameters = new();

        if (!Match(TokenType.RightParen))
        {
            do
            {
                ConsumeType();
                var paramName = Consume(TokenType.Identifier, "Expected a parameter name");
                parameters.Add(paramName.Value);
            } while (Match(TokenType.Comma) && Advance() != null);
        }

        Consume(TokenType.RightParen, "Expected ')' after parameters");

        var body = ParseBlock();
        return AtStatement(
            new FunctionDeclaration(name.Value, returnType.Value, parameters, body),
            returnType.Location);
    }

    private ReturnStatement ParseReturn()
    {
        var token = Advance(); // return

        Expression? value = null;

        if (!Match(TokenType.Semicolon)) value = ParseExpression();

        Consume(TokenType.Semicolon, "Expected ';' after return statement");
        return AtStatement(new ReturnStatement(value), token.Location);
    }

    private static readonly HashSet<TokenType> TypeTokens = new()
    {
        TokenType.I8,
        TokenType.I16,
        TokenType.I32,
        TokenType.I64,

        TokenType.UI8,
        TokenType.UI16,
        TokenType.UI32,
        TokenType.UI64,

        TokenType.F16,
        TokenType.F32,
        TokenType.F64,

        TokenType.UF16,
        TokenType.UF32,
        TokenType.UF64,

        TokenType.Bool,
        TokenType.Char,
        TokenType.Str,
        TokenType.Void
    };

    private static bool IsType(Token token)
    {
        return TypeTokens.Contains(token.Type);
    }

    private Token ConsumeType()
    {
        if (!IsType(Current))
            throw new ParserException($"Expected a type but found '{Current.Value}'.", Current.Location);

        return Advance();
    }

    private PrintStatement ParsePrintln()
    {
        var token = Advance(); // println
        Consume(TokenType.LeftParen, "Expected '(' after println");

        List<Expression> arguments = new();

        if (!Match(TokenType.RightParen))
        {
            do
            {
                arguments.Add(ParseExpression());
            } while (Match(TokenType.Comma) && Advance() != null);
        }

        Consume(TokenType.RightParen, "Expected ')' after println arguments");
        Consume(TokenType.Semicolon, "Expected ';' after println");
        return AtStatement(new PrintStatement(arguments), token.Location);
    }

    private IfStatement ParseIf()
    {
        var token = Advance(); // if
        Consume(TokenType.LeftParen, "Expected '(' after if");

        var condition = ParseExpression();

        Consume(TokenType.RightParen, "Expected ')' after if condition");

        List<Statement> thenBranch = ParseBlock();
        List<(Expression Condition, List<Statement> Body)> elifBranches = new();
        List<Statement>? elseBranch = null;

        while (Match(TokenType.Elif))
        {
            Advance(); // elif
            Consume(TokenType.LeftParen, "Expected '(' after elif");

            var elifCondition = ParseExpression();

            Consume(TokenType.RightParen, "Expected ')' after elif condition");

            var body = ParseBlock();
            elifBranches.Add((elifCondition, body));
        }

        if (Match(TokenType.Else))
        {
            Advance(); // else
            elseBranch = ParseBlock();
        }

        return AtStatement(
            new IfStatement(condition, thenBranch, elifBranches, elseBranch),
            token.Location);
    }

    private WhileStatement ParseWhile()
    {
        var token = Advance(); // while
        Consume(TokenType.LeftParen, "Expected '(' after while");

        var condition = ParseExpression();

        Consume(TokenType.RightParen, "Expected ')' after while condition");

        var body = ParseBlock();
        return AtStatement(new WhileStatement(condition, body), token.Location);
    }

    private List<Statement> ParseBlock()
    {
        Consume(TokenType.LeftBrace, "Expected '{' to start a block");

        List<Statement> body = new();
        while (!Match(TokenType.RightBrace) && !IsAtEnd())
            body.Add(ParseStatement());

        Consume(TokenType.RightBrace, "Expected '}' to end a block");
        return body;
    }

    private Statement ParseAssignment()
    {
        var name = Consume(TokenType.Identifier, "Expected a variable name");
        Consume(TokenType.Equals, "Expected '=' in assignment");

        var value = ParseExpression();

        Consume(TokenType.Semicolon, "Expected ';' after assignment");
        return AtStatement(new AssignmentStatement(name.Value, value), name.Location);
    }

    private Expression ParseExpression()
    {
        return ParseEquality();
    }

    private Expression ParseEquality()
    {
        var left = ParseComparison();

        while (Match(TokenType.EqualEqual) || Match(TokenType.NotEqual))
        {
            var op = Advance();
            var right = ParseComparison();
            left = AtExpression(new BinaryExpression(left, op.Value, right), op.Location);
        }

        return left;
    }

    private Expression ParseComparison()
    {
        var left = ParseTerm();

        while (Match(TokenType.Less) || Match(TokenType.LessEqual) ||
               Match(TokenType.Greater) || Match(TokenType.GreaterEqual))
        {
            var op = Advance();
            var right = ParseTerm();
            left = AtExpression(new BinaryExpression(left, op.Value, right), op.Location);
        }

        return left;
    }

    private Expression ParseTerm()
    {
        var left = ParseFactor();

        while (Match(TokenType.Plus) || Match(TokenType.Minus))
        {
            var op = Advance();
            var right = ParseFactor();
            left = AtExpression(new BinaryExpression(left, op.Value, right), op.Location);
        }

        return left;
    }

    private Expression ParseFactor()
    {
        var left = ParseUnary();

        while (Match(TokenType.Star) || Match(TokenType.Slash) || Match(TokenType.Percent))
        {
            var op = Advance();
            var right = ParseUnary();
            left = AtExpression(new BinaryExpression(left, op.Value, right), op.Location);
        }

        return left;
    }

    private Expression ParseUnary()
    {
        if (Match(TokenType.Minus) || Match(TokenType.Bang))
        {
            var op = Advance();
            return AtExpression(new UnaryExpression(op.Value, ParseUnary()), op.Location);
        }

        return ParsePrimary();
    }

    private Expression ParsePrimary()
    {
        if (Match(TokenType.LeftParen))
        {
            Advance(); // (
            var expression = ParseExpression();
            Consume(TokenType.RightParen, "Expected ')' after expression");
            return expression;
        }

        if (Match(TokenType.Number))
        {
            var token = Advance();
            var kind = token.Value.Contains('.') ? LiteralKind.Float : LiteralKind.Integer;
            return AtExpression(new LiteralExpression(token.Value, kind), token.Location);
        }

        if (Match(TokenType.String))
        {
            var token = Advance();
            return AtExpression(new LiteralExpression(token.Value, LiteralKind.String), token.Location);
        }

        if (Match(TokenType.CharLiteral))
        {
            var token = Advance();
            return AtExpression(new LiteralExpression(token.Value, LiteralKind.Character), token.Location);
        }

        if (Match(TokenType.True) || Match(TokenType.False))
        {
            var token = Advance();
            return AtExpression(new LiteralExpression(token.Value, LiteralKind.Boolean), token.Location);
        }

        if (Match(TokenType.Identifier))
        {
            var name = Advance();

            if (Match(TokenType.LeftParen))
                return ParseCall(name);

            if (Match(TokenType.LeftSquareBrace))
            {
                Advance(); // [

                var x = ParseExpression();

                Consume(TokenType.Comma, "Expected ',' between array indices");
                var y = ParseExpression();

                Consume(TokenType.RightSquareBrace, "Expected ']' after array indices");
                return AtExpression(new ArrayAccess(name.Value, x, y), name.Location);
            }

            return AtExpression(new VariableExpression(name.Value), name.Location);
        }

        throw new ParserException($"Unexpected token '{Current.Value}' in expression.", Current.Location);
    }

    private CallExpression ParseCall(Token name)
    {
        Consume(TokenType.LeftParen, "Expected '(' after function name");

        List<Expression> arguments = new();

        if (!Match(TokenType.RightParen))
        {
            do
            {
                arguments.Add(ParseExpression());
            } while (Match(TokenType.Comma) && Advance() != null);
        }

        Consume(TokenType.RightParen, "Expected ')' after arguments");
        return AtExpression(new CallExpression(name.Value, arguments), name.Location);
    }

    private Statement ParseStatement()
    {
        if (Match(TokenType.Var) || Match(TokenType.Const))
            return ParseVarDeclaration();

        if (Current.Type == TokenType.Identifier && PeekType(1) == TokenType.Equals)
            return ParseAssignment();

        if (Current.Type == TokenType.Identifier && PeekType(1) == TokenType.LeftParen)
            return ParseCallStatement();

        if (Match(TokenType.Println)) return ParsePrintln();
        if (Match(TokenType.If)) return ParseIf();
        if (Match(TokenType.While)) return ParseWhile();
        if (Match(TokenType.Return)) return ParseReturn();

        if (IsType(Current) && PeekType(1) == TokenType.Identifier)
        {
            if (PeekType(2) == TokenType.LeftParen) return ParseFunction();
            if (PeekType(2) == TokenType.Equals) return ParseTypedVarDeclaration();
        }

        throw new ParserException($"Unknown statement '{Current.Value}'.", Current.Location);
    }

    private Token Current => _tokens[Math.Min(_position, _tokens.Count - 1)];

    private TokenType PeekType(int offset)
    {
        int index = _position + offset;
        return index < _tokens.Count ? _tokens[index].Type : TokenType.EOF;
    }

    private Token Advance()
    {
        if (IsAtEnd())
            throw new ParserException("Unexpected end of input.", Current.Location);

        return _tokens[_position++];
    }

    private bool Match(TokenType type)
    {
        return Current.Type == type;
    }

    private Token Consume(TokenType type, string message)
    {
        if (!Match(type))
            throw new ParserException($"{message}. Found '{Current.Value}'.", Current.Location);

        return Advance();
    }

    private bool IsAtEnd()
    {
        return _position >= _tokens.Count || _tokens[_position].Type == TokenType.EOF;
    }
}
