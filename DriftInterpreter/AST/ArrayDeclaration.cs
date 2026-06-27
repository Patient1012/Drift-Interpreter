public class ArrayDeclaration : Statement
{
    public string Name;
    public List<int> Dimensions;

    public ArrayDeclaration(string name, List<int> dimensions)
    {
        Name = name;
        Dimensions = dimensions;
    }
}