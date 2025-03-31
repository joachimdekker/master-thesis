using ExcelCompiler.Domain.Spreadsheet;

namespace ExcelCompiler.Domain.Compute;

public class Range : ComputeUnit
{
    public Location From { get; }
    public Location To { get; }
    
    public Range(Location from, Location to, Location cell) : base(cell)
    {
        if (from.WorksheetIndex != to.WorksheetIndex) 
            throw new ArgumentException("From and To locations must be in the same worksheet.");
        
        From = from;
        To = to;
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
                    WorksheetIndex = From.WorksheetIndex,
                };
            }
        }
    }

    public static Range FromString(string range, Location location)
    {
        // Get the first and last cell in the range
        var cells = range.Split(':');
        if (cells.Length != 2)
            throw new ArgumentException("Invalid range format. Expected format: A1:B2");
        
        var from = Location.FromA1(cells[0]);
        var to = Location.FromA1(cells[1]);
        
        return new Range(from, to, location);
    }
}