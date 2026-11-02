public class Token {
    public TokenType Type { get; }
    public string Value { get; }
    public SourcePosition Location { get; }

    public Token(TokenType type, string value, SourcePosition location) {
        Type = type;
        Value = value;
        Location = location;
    }

    public override string ToString() {
        return $"{Type}: {Value} ({Location})";
    }
}
