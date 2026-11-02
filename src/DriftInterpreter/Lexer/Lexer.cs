using System.Text;

public class Lexer
{
    private readonly string _source;
    private readonly List<int> _lineStarts = new() { 0 };
    private int _position;

    public Lexer(string source)
    {
        _source = source;

        for (int i = 0; i < source.Length; i++)
        {
            if (source[i] == '\n')
                _lineStarts.Add(i + 1);
        }
    }

    private char Peek(int offset = 1)
    {
        int index = _position + offset;
        if (index >= _source.Length)
            return '\0';

        return _source[index];
    }

    private SourcePosition PositionAt(int offset)
    {
        int index = _lineStarts.BinarySearch(offset);
        if (index < 0)
            index = ~index - 1;

        if (index < 0)
            index = 0;

        return new SourcePosition(offset, index + 1, offset - _lineStarts[index] + 1);
    }

    private Token MakeToken(TokenType type, string value, int start)
    {
        return new Token(type, value, PositionAt(start));
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

            if (current == '/' && Peek() == '/')
            {
                SkipLineComment();
                continue;
            }

            if (current == '/' && Peek() == '*')
            {
                SkipBlockComment(_position);
                continue;
            }

            if (char.IsLetter(current) || current == '_')
            {
                int start = _position;
                tokens.Add(ReadIdentifier(start));
                continue;
            }

            if (char.IsDigit(current))
            {
                int start = _position;
                tokens.Add(ReadNumber(start));
                continue;
            }

            if (current == '"')
            {
                int start = _position;
                tokens.Add(ReadString(start));
                continue;
            }

            if (current == '\'')
            {
                int start = _position;
                tokens.Add(ReadChar(start));
                continue;
            }

            int tokenStart = _position;

            switch (current)
            {
                case '+':
                    tokens.Add(MakeToken(TokenType.Plus, "+", tokenStart));
                    break;

                case '-':
                    tokens.Add(MakeToken(TokenType.Minus, "-", tokenStart));
                    break;

                case '*':
                    tokens.Add(MakeToken(TokenType.Star, "*", tokenStart));
                    break;

                case '/':
                    tokens.Add(MakeToken(TokenType.Slash, "/", tokenStart));
                    break;

                case '%':
                    tokens.Add(MakeToken(TokenType.Percent, "%", tokenStart));
                    break;

                case '{':
                    tokens.Add(MakeToken(TokenType.LeftBrace, "{", tokenStart));
                    break;

                case '}':
                    tokens.Add(MakeToken(TokenType.RightBrace, "}", tokenStart));
                    break;

                case '=':
                    if (Peek() == '=')
                    {
                        tokens.Add(MakeToken(TokenType.EqualEqual, "==", tokenStart));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(MakeToken(TokenType.Equals, "=", tokenStart));
                    }
                    break;

                case '!':
                    if (Peek() == '=')
                    {
                        tokens.Add(MakeToken(TokenType.NotEqual, "!=", tokenStart));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(MakeToken(TokenType.Bang, "!", tokenStart));
                    }
                    break;

                case '>':
                    if (Peek() == '=')
                    {
                        tokens.Add(MakeToken(TokenType.GreaterEqual, ">=", tokenStart));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(MakeToken(TokenType.Greater, ">", tokenStart));
                    }
                    break;

                case '<':
                    if (Peek() == '=')
                    {
                        tokens.Add(MakeToken(TokenType.LessEqual, "<=", tokenStart));
                        _position++;
                    }
                    else
                    {
                        tokens.Add(MakeToken(TokenType.Less, "<", tokenStart));
                    }
                    break;

                case ';':
                    tokens.Add(MakeToken(TokenType.Semicolon, ";", tokenStart));
                    break;

                case '(':
                    tokens.Add(MakeToken(TokenType.LeftParen, "(", tokenStart));
                    break;

                case ')':
                    tokens.Add(MakeToken(TokenType.RightParen, ")", tokenStart));
                    break;

                case '[':
                    tokens.Add(MakeToken(TokenType.LeftSquareBrace, "[", tokenStart));
                    break;

                case ']':
                    tokens.Add(MakeToken(TokenType.RightSquareBrace, "]", tokenStart));
                    break;

                case ',':
                    tokens.Add(MakeToken(TokenType.Comma, ",", tokenStart));
                    break;

                default:
                    throw new LexerException(
                        $"Unexpected character '{current}'.",
                        PositionAt(tokenStart));
            }
            _position++;
        }

        tokens.Add(new Token(TokenType.EOF, "", PositionAt(_position)));
        return tokens;
    }

    private void SkipLineComment()
    {
        _position += 2;

        while (_position < _source.Length && _source[_position] != '\n')
            _position++;
    }

    private void SkipBlockComment(int start)
    {
        _position += 2;

        while (_position < _source.Length)
        {
            if (_source[_position] == '*' && Peek() == '/')
            {
                _position += 2;
                return;
            }

            _position++;
        }

        throw new LexerException("Unterminated block comment.", PositionAt(start));
    }

    private Token ReadIdentifier(int start)
    {
        while (_position < _source.Length &&
               (char.IsLetterOrDigit(_source[_position])
                || _source[_position] == '_'))
        {
            _position++;
        }
        string text = _source[start.._position];

        return text switch
        {
            "var" => MakeToken(TokenType.Var, text, start),
            "const" => MakeToken(TokenType.Const, text, start),
            "println" => MakeToken(TokenType.Println, text, start),
            "readln" => MakeToken(TokenType.Readln, text, start),

            "true" => MakeToken(TokenType.True, text, start),
            "false" => MakeToken(TokenType.False, text, start),

            "i8" => MakeToken(TokenType.I8, text, start),
            "i16" => MakeToken(TokenType.I16, text, start),
            "i32" => MakeToken(TokenType.I32, text, start),
            "i64" => MakeToken(TokenType.I64, text, start),

            "ui8" => MakeToken(TokenType.UI8, text, start),
            "ui16" => MakeToken(TokenType.UI16, text, start),
            "ui32" => MakeToken(TokenType.UI32, text, start),
            "ui64" => MakeToken(TokenType.UI64, text, start),

            "f16" => MakeToken(TokenType.F16, text, start),
            "f32" => MakeToken(TokenType.F32, text, start),
            "f64" => MakeToken(TokenType.F64, text, start),

            "uf16" => MakeToken(TokenType.UF16, text, start),
            "uf32" => MakeToken(TokenType.UF32, text, start),
            "uf64" => MakeToken(TokenType.UF64, text, start),

            "str" => MakeToken(TokenType.Str, text, start),
            "bool" => MakeToken(TokenType.Bool, text, start),
            "char" => MakeToken(TokenType.Char, text, start),
            "void" => MakeToken(TokenType.Void, text, start),

            "if" => MakeToken(TokenType.If, text, start),
            "elif" => MakeToken(TokenType.Elif, text, start),
            "else" => MakeToken(TokenType.Else, text, start),
            "switch" => MakeToken(TokenType.Switch, text, start),
            "case" => MakeToken(TokenType.Case, text, start),

            "while" => MakeToken(TokenType.While, text, start),
            "for" => MakeToken(TokenType.For, text, start),
            "do" => MakeToken(TokenType.Do, text, start),
            "break" => MakeToken(TokenType.Break, text, start),

            "return" => MakeToken(TokenType.Return, text, start),

            _ => MakeToken(TokenType.Identifier, text, start)
        };
    }

    private Token ReadNumber(int start)
    {
        while (_position < _source.Length && char.IsDigit(_source[_position]))
        {
            _position++;
        }

        if (_position < _source.Length && _source[_position] == '.')
        {
            _position++;

            while (_position < _source.Length && char.IsDigit(_source[_position]))
                _position++;
        }

        string number = _source[start.._position];

        return MakeToken(TokenType.Number, number, start);
    }

    private Token ReadString(int start)
    {
        _position++;
        var value = new StringBuilder();

        while (_position < _source.Length && _source[_position] != '"')
        {
            if (_source[_position] == '\\')
            {
                _position++;
                value.Append(ReadEscape(start));
                continue;
            }

            value.Append(_source[_position]);
            _position++;
        }

        if (_position >= _source.Length)
            throw new LexerException("Unterminated string literal.", PositionAt(start));

        _position++;
        return MakeToken(TokenType.String, value.ToString(), start);
    }

    private Token ReadChar(int start)
    {
        _position++;

        if (_position >= _source.Length || _source[_position] == '\'')
            throw new LexerException("Empty character literal.", PositionAt(start));

        char value;

        if (_source[_position] == '\\')
        {
            _position++;
            value = ReadEscape(start);
        }
        else
        {
            value = _source[_position];
            _position++;
        }

        if (_position >= _source.Length || _source[_position] != '\'')
            throw new LexerException("Unterminated character literal.", PositionAt(start));

        _position++;
        return MakeToken(TokenType.CharLiteral, value.ToString(), start);
    }

    private char ReadEscape(int start)
    {
        if (_position >= _source.Length)
            throw new LexerException("Unterminated escape sequence.", PositionAt(start));

        char escaped = _source[_position];
        _position++;

        return escaped switch
        {
            'n' => '\n',
            'r' => '\r',
            't' => '\t',
            '0' => '\0',
            '\\' => '\\',
            '\'' => '\'',
            '"' => '"',
            _ => escaped
        };
    }
}
