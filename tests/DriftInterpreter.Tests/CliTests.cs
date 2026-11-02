using Xunit;

public class CliTests
{
    [Fact]
    public void RunsFileAndReturnsSuccess()
    {
        var path = WriteTempProgram(
            """
            void main()
            {
                println("ok");
            }
            """);

        try
        {
            var result = Invoke(path);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal("ok\n", result.StdOut);
            Assert.Equal(string.Empty, result.StdErr);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReportsSourceLocatedRuntimeErrorAndExitCode()
    {
        var path = WriteTempProgram("println(missing);");

        try
        {
            var result = Invoke(path);

            Assert.Equal(1, result.ExitCode);
            Assert.Equal(string.Empty, result.StdOut);
            Assert.Equal(
                $"{path}:1:9: error: Undefined variable 'missing'.\n",
                result.StdErr);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void ReportsMissingFile()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"drift-missing-{Guid.NewGuid():N}.drf");

        var result = Invoke(path);

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Could not read", result.StdErr);
    }

    [Fact]
    public void PrintsUsageWhenNoFileIsGiven()
    {
        var result = Invoke();

        Assert.Equal(1, result.ExitCode);
        Assert.Contains("Usage: drift <file.drf>", result.StdErr);
    }

    private static string WriteTempProgram(string source)
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"drift-test-{Guid.NewGuid():N}.drf");

        File.WriteAllText(path, source);
        return path;
    }

    private static (int ExitCode, string StdOut, string StdErr) Invoke(
        params string[] arguments)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var previousOutput = Console.Out;
        var previousError = Console.Error;

        Console.SetOut(output);
        Console.SetError(error);

        try
        {
            int exitCode = Program.Main(arguments);
            return (
                exitCode,
                Normalize(output.ToString()),
                Normalize(error.ToString()));
        }
        finally
        {
            Console.SetOut(previousOutput);
            Console.SetError(previousError);
        }
    }

    private static string Normalize(string value)
    {
        return value.Replace("\r\n", "\n");
    }
}
