using Xunit;

public class FormattingTests
{
    [Fact]
    public void FormatsIntegerAndEscapedPercent()
    {
        var output = TestRunner.Run("""println("%i%%", 42);""");
        Assert.Equal("42%\n", output);
    }

    [Fact]
    public void FormatsSupportedSpecifiers()
    {
        var output = TestRunner.Run("""println("%d %f %s %c %b %v", 7, 2.5, "hi", 'A', true, 9);""");
        Assert.Equal("7 2.5 hi A true 9\n", output);
    }

    [Fact]
    public void SingleStringStillEscapesPercent()
    {
        var output = TestRunner.Run("""println("100%% complete");""");
        Assert.Equal("100% complete\n", output);
    }

    [Fact]
    public void MissingArgumentReportsStringLocation()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """println("value %i");""");

        Assert.Equal(1, exception.Location.Line);
        Assert.Equal(9, exception.Location.Column);
        Assert.Contains("requires an argument", exception.Message);
    }

    [Fact]
    public void UnknownSpecifierReportsError()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """println("%q", 1);""");

        Assert.Contains("Unknown format specifier '%q'", exception.Message);
    }

    [Fact]
    public void ExtraArgumentsReportError()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """println("%i", 1, 2);""");

        Assert.Contains("Too many arguments", exception.Message);
    }

    [Fact]
    public void IntegerSpecifierRejectsString()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """println("%i", "not an int");""");

        Assert.Contains("requires an integer", exception.Message);
    }

    [Fact]
    public void MultipleNonStringArgumentsReportError()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            "println(1, 2);");

        Assert.Contains("format string", exception.Message);
    }
}
