namespace ExcelCompiler.Representations.Structure;

public class Table
{
    public string Name { get; }
    
    public Dictionary<string, Range> Columns { get; init; } = [];
    
    public Range Location { get; set; }
    
    public Table(string name)
    {
        Name = name;
    }
}