public class LiteralExpression : Expression
{
    public string Text;

    public LiteralExpression(string text)
    {
        Text = text;
    }
}