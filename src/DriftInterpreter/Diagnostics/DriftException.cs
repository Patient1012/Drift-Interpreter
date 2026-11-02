public class DriftException : Exception
{
    public DiagnosticPhase Phase { get; }
    public SourcePosition Location { get; }

    public DriftException(DiagnosticPhase phase, string message, SourcePosition location)
        : base(message)
    {
        Phase = phase;
        Location = location;
    }

    public string ToDiagnosticString(string? file = null)
    {
        var prefix = string.IsNullOrWhiteSpace(file) ? string.Empty : $"{file}:";

        if (Location.IsKnown)
            return $"{prefix}{Location.Line}:{Location.Column}: error: {Message}";

        return $"{prefix}error: {Message}";
    }
}

public sealed class LexerException : DriftException
{
    public LexerException(string message, SourcePosition location)
        : base(DiagnosticPhase.Lexer, message, location)
    {
    }
}

public sealed class ParserException : DriftException
{
    public ParserException(string message, SourcePosition location)
        : base(DiagnosticPhase.Parser, message, location)
    {
    }
}

public sealed class SemanticException : DriftException
{
    public SemanticException(string message, SourcePosition location)
        : base(DiagnosticPhase.Semantic, message, location)
    {
    }
}

public sealed class RuntimeException : DriftException
{
    public RuntimeException(string message, SourcePosition location)
        : base(DiagnosticPhase.Runtime, message, location)
    {
    }
}
