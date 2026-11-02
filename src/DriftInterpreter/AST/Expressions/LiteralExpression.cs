public enum LiteralKind
{
    Integer,
    Float,
    String,
    Boolean,
    Character
}

public class LiteralExpression : Expression
{
    public string Text;
    public LiteralKind Kind;

    public LiteralExpression(string text, LiteralKind kind = LiteralKind.Integer)
    {
        Text = text;
        Kind = kind;
    }
}
