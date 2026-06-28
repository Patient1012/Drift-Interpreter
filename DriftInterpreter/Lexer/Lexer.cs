public class Lexer
{
    private readonly string _source;
    private int _position;

    public Lexer(string source)
    {
        _source = source;
    }

    private char Peek()
    {
        if (_position + 1 >= _source.Length)
            return '\0';

        return _source[_position + 1];
    }

    public List<Token> Tokenize()
    {
        List<Token> tokens = new();

        while (_position < _source.Length)
        {
            char current = _source[_position];

            if (char.IsWhiteSpace(current))
            {
                _position++;
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                tokens.Add(ReadIdentifier());
                continue;
            }

            if (char.IsDigit(current))
            {
                tokens.Add(ReadNumber());
                continue;
            }

            if (current == '"')
            {
                tokens.Add(ReadString());
                continue;
            }

            switch (current)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+"));
                    break;

                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-"));
                    break;

                case '*':
                    tokens.Add(new Token(TokenType.Star, "*"));
                    break;

                case '/':
                    tokens.Add(new Token(TokenType.Slash, "/"));
                    break;

                case '{':
                    tokens.Add(new Token(TokenType.LeftBrace, "{"));
                    break;

                case '}':
                    tokens.Add(new Token(TokenType.RightBrace, "}"));
                    break;

                case '=':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.EqualEqual, "=="));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Equals, "="));
                    }
                    break;

                case '>':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.GreaterEqual, ">="));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Greater, ">"));
                    }
                    break;

                case '<':
                    if (Peek() == '=')
                    {
                        tokens.Add(new Token(TokenType.LessEqual, "<="));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(new Token(TokenType.Less, "<"));
                    }
                    break;

                case ';':
                    tokens.Add(new Token(TokenType.Semicolon, ";"));
                    break;

                case '(':
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                    break;

                case ')':
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                    break;
                
                case '[':
                    tokens.Add(new Token(TokenType.LeftSquareBrace, "["));
                    break;
                
                case ']':
                    tokens.Add(new Token(TokenType.RightSquareBrace, "]"));
                    break;

                case ',':
                    tokens.Add(new Token(TokenType.Comma, ","));
                    break;

                default:
                    throw new Exception($"Unexpected character: {current}");
            }
            _position++;
        }
        tokens.Add(new Token(TokenType.EOF, ""));
        return tokens;
    }

    private Token ReadIdentifier()
    {
        int start = _position;

        while (_position < _source.Length &&
               (char.IsLetterOrDigit(_source[_position])
                || _source[_position] == '_'))
        {
            _position++;
        }
        string text = _source[start.._position];

        return text switch
        {
            "var" => new Token(TokenType.Var, text),
            "const" => new Token(TokenType.Const, text),
            "println" => new Token(TokenType.Println, text),
            "readln" => new Token(TokenType.Readln, text),

            "i8" => new Token(TokenType.I8, text),
            "i16" => new Token(TokenType.I16, text),
            "i32" => new Token(TokenType.I32, text),
            "i64" => new Token(TokenType.I64, text),
            
            "ui8" => new Token(TokenType.UI8, text),
            "ui16" => new Token(TokenType.UI16, text),
            "ui32" => new Token(TokenType.UI32, text),
            "ui64" => new Token(TokenType.UI64, text),

            "f16" => new Token(TokenType.F16, text),
            "f32" => new Token(TokenType.F32, text),
            "f64" => new Token(TokenType.F64, text),
            
            "uf16" => new Token(TokenType.UF16, text),
            "uf32" => new Token(TokenType.UF32, text),
            "uf64" => new Token(TokenType.UF64, text),

            "str" => new Token(TokenType.Str, text),
            "bool" => new Token(TokenType.Bool, text),
            "char" => new Token(TokenType.Char, text),
            "void" => new Token(TokenType.Void, text),

            "if" => new Token(TokenType.If, text),
            "elif" => new Token(TokenType.Elif, text),
            "else" => new Token(TokenType.Else, text),
            "switch" => new Token(TokenType.Switch,text),
            "case" => new Token(TokenType.Case, text),


            "while" => new Token(TokenType.While, text),
            "for" => new Token(TokenType.For, text),
            "do" => new Token(TokenType.Do, text),
            "break" => new Token(TokenType.Break, text),

            "return" => new Token(TokenType.Return, text),

            _ => new Token(TokenType.Identifier, text)
        };
    }

    private Token ReadNumber()
    {
        int start = _position;

        while (_position < _source.Length && char.IsDigit(_source[_position]))
        {
            _position++;
        }
        string number = _source[start.._position];

        return new Token(TokenType.Number, number);
    }

    private Token ReadString()
    {
        _position++;
        int start = _position;

        while (_position < _source.Length && _source[_position] != '"')
        {
            _position++;
        }
        string value = _source[start.._position];
        _position++;
        return new Token(TokenType.String, value);
    }

}
