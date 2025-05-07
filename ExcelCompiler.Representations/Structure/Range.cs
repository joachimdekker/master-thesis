using ExcelCompiler.Domain.Structure;

namespace ExcelCompiler.Representations.Structure;

public record Range : Reference
{
    public string? Spreadsheet => From.Spreadsheet;
    public Location From { get; init;  }
    public Location To { get; init; }
    
    public Range(Location from, Location to)
    {
        if (from.Spreadsheet != to.Spreadsheet) 
            throw new ArgumentException("From and To locations must be in the same worksheet.");
        
        From = from;
        To = to;
    }

    public bool Contains(Location location)
    {
        if (location.Spreadsheet != From.Spreadsheet)
            return false;
        
        return location.Row >= From.Row && location.Row <= To.Row && location.Column >= From.Column && location.Column <= To.Column;
    }
    
    public IEnumerable<Location> GetLocations()
    {
        // Generate all locations in the range
        for (int row = From.Row; row <= To.Row; row++)
        {
            for (int col = From.Column; col <= To.Column; col++)
            {
                yield return new Location
                {
                    Column = col,
                    Row = row,
                    Spreadsheet = From.Spreadsheet,
                };
            }
        }
    }

    public static Range FromString(string range, string? spreadsheet = null)
    {
        // Get the first and last cell in the range
        var cells = range.Split(':');
        if (cells.Length != 2)
            throw new ArgumentException("Invalid range format. Expected format: A1:B2");
        
        var from = Location.FromA1(cells[0], spreadsheet);
        var to = Location.FromA1(cells[1], spreadsheet);
        
        return new Range(from, to);
    }
}