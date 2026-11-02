using Xunit;

public class EntryPointTests
{
    [Fact]
    public void RunsMainAutomatically()
    {
        var output = TestRunner.Run(
            """
            void main()
            {
                println("ran");
            }
            """);

        Assert.Equal("ran\n", output);
    }

    [Fact]
    public void FindsMainCaseInsensitively()
    {
        var output = TestRunner.Run(
            """
            void mAin()
            {
                println("ran");
            }
            """);

        Assert.Equal("ran\n", output);
    }

    [Fact]
    public void DoesNotRunMainTwiceWhenCalledExplicitly()
    {
        var output = TestRunner.Run(
            """
            void Main()
            {
                println("once");
            }

            Main();
            """);

        Assert.Equal("once\n", output);
    }

    [Fact]
    public void AllowsNonVoidMain()
    {
        var output = TestRunner.Run(
            """
            i32 main()
            {
                println("main");
                return 0;
            }
            """);

        Assert.Equal("main\n", output);
    }

    [Fact]
    public void RejectsMainWithParameters()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """
            void main(i32 value)
            {
                println(value);
            }
            """);

        Assert.Equal(1, exception.Location.Line);
        Assert.Contains("must not declare parameters", exception.Message);
    }

    [Fact]
    public void RejectsAmbiguousEntryPoints()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """
            void main()
            {
            }

            void Main()
            {
            }
            """);

        Assert.Contains("Multiple entry points", exception.Message);
    }
}
