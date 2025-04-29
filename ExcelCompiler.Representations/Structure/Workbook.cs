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

    public List<Table> Tables { get; init; } = [];

    public Cell GetCell(Location location)
    {
        if (location.Spreadsheet is null)
            throw new ArgumentException("Location must have a spreadsheet.");
        
        Spreadsheet spreadsheet = Spreadsheets.Single(sp => sp.Name == location.Spreadsheet);
        return spreadsheet[location];
    }
}