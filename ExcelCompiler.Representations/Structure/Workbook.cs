using ExcelCompiler.Domain.Structure;

namespace ExcelCompiler.Representations.Structure;

public class Workbook
{
    public string Name { get; }
    
    public Workbook(string name)
    {
        Name = name;
    }
    
    public Cell this[Location location] => GetCell(location);
    
    public List<Spreadsheet> Spreadsheets { get; } = [];
    
    public Dictionary<string, Reference> NamedRanges { get; init; } = [];

    public IEnumerable<Table> Tables => Spreadsheets.SelectMany(sp => sp.Tables);

    public Cell GetCell(Location location)
    {
        if (location.Spreadsheet is null)
            throw new ArgumentException("Location must have a spreadsheet.");
        
        Spreadsheet spreadsheet = Spreadsheets.Single(sp => sp.Name == location.Spreadsheet);
        return spreadsheet[location];
    }
}