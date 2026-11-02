using Xunit;

public class InterpreterTests
{
    [Fact]
    public void ExecutesArithmeticWithPrecedence()
    {
        var output = TestRunner.Run(
            """
            println(1 + 2 * 3);
            println((1 + 2) * 3);
            println(10 % 3);
            """);

        Assert.Equal("7\n9\n1\n", output);
    }

    [Fact]
    public void SupportsFunctionsAndScope()
    {
        var output = TestRunner.Run(
            """
            i32 add(i32 a, i32 b)
            {
                return a + b;
            }

            println(add(2, 3));
            """);

        Assert.Equal("5\n", output);
    }

    [Fact]
    public void SupportsCommentsAndControlFlow()
    {
        var output = TestRunner.Run(
            """
            // line comment
            /* block comment */
            var i32 x = 2;
            if (x == 2)
            {
                println("two");
            }
            else
            {
                println("other");
            }
            """);

        Assert.Equal("two\n", output);
    }

    [Fact]
    public void BlockScopeHidesInnerVariables()
    {
        var exception = TestRunner.CaptureException<RuntimeException>(
            """
            if (true)
            {
                var i32 inner = 1;
            }

            println(inner);
            """);

        Assert.Contains("Undefined variable 'inner'", exception.Message);
    }
}
