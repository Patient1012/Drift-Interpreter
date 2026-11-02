public class ArrayValue
{
    public int Width;
    public int Height;
    public object[] Data;

    public ArrayValue(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new object[width * height];
    }

    public int Index(int x, int y)
    {
        return x + (y * Width);
    }
}