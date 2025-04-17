namespace ExcelCompiler.Domain.Structure;

public class Workbook
{
    public string Name { get; }
    
    public Workbook(string name)
    {
        Name = name;
    }
    
    public List<Spreadsheet> Spreadsheets { get; } = [];
    
    public Dictionary<string, Reference> NamedRanges { get; init; } = [];

    public Cell GetCell(Location location)
    {
        return location.Spreadsheet[location];
    }
}