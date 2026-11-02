public enum DiagnosticPhase
{
    Lexer,
    Parser,
    Semantic,
    Runtime
}

public readonly struct SourcePosition
{
    public int Offset { get; }
    public int Line { get; }
    public int Column { get; }

    public SourcePosition(int offset, int line, int column)
    {
        Offset = offset;
        Line = line;
        Column = column;
    }

    public static SourcePosition None => default;

    public bool IsKnown => Line > 0;

    public override string ToString()
    {
        return IsKnown ? $"{Line}:{Column}" : "<unknown>";
    }
}
