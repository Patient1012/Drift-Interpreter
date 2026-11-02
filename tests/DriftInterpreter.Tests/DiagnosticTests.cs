using Xunit;

public class DiagnosticTests
{
    [Fact]
    public void LexerReportsLineAndColumn()
    {
        var exception = Assert.Throws<LexerException>(
            () => new Lexer("var i32 x = 1;\nvar i32 y = @;").Tokenize());

        Assert.Equal(DiagnosticPhase.Lexer, exception.Phase);
        Assert.Equal(2, exception.Location.Line);
        Assert.Equal(13, exception.Location.Column);
        Assert.Contains("@", exception.Message);
    }

    [Fact]
    public void ParserReportsLineAndColumn()
    {
        var exception = TestRunner.CaptureException<ParserException>(
            """
            var i32 x = 1;
            var i32 y = ;
            """);

        Assert.Equal(DiagnosticPhase.Parser, exception.Phase);
        Assert.Equal(2, exception.Location.Line);
        Assert.Equal(13, exception.Location.Column);
    }

    [Fact]
    public void RuntimeReportsExpressionLocation()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """
            var i32 x = 1;
            println(missing);
            """);

        Assert.Equal(DiagnosticPhase.Runtime, exception.Phase);
        Assert.Equal(2, exception.Location.Line);
        Assert.Equal(9, exception.Location.Column);
        Assert.Contains("Undefined variable 'missing'", exception.Message);
    }

    [Fact]
    public void DiagnosticStringIncludesFileLineAndColumn()
    {
        var exception = new ParserException(
            "Expected a thing.",
            new SourcePosition(0, 4, 7));

        Assert.Equal(
            "example.drf:4:7: error: Expected a thing.",
            exception.ToDiagnosticString("example.drf"));
    }
}
