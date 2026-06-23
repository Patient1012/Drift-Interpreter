public class ArrayDeclaration : Statement
{
    public string Name;
    public int Width;
    public int Height;

    public ArrayDeclaration(string name, int width, int height)
    {
        Name = name;
        Width = width;
        Height = height;
    }
}