using System.Collections;
using ExcelCompiler.Representations.Structure;

namespace ExcelCompiler.Representations.References;

public record Range : Reference, IEnumerable<Location>
{
    public string? Spreadsheet => From.Spreadsheet;
    public Location From { get; init;  }
    public Location To { get; init; }

    public int Width => To.Column - From.Column + 1;
    public int Height => To.Row - From.Row + 1;

    public override bool IsSingleReference => From == To;
    
    public Range(Location from, Location to)
    {
        if (from.Spreadsheet != to.Spreadsheet) 
            throw new ArgumentException("From and To locations must be in the same worksheet.");
        
        From = from;
        To = to;
    }

    public Location[,] ToArray()
    {
        Location[,] result = new Location[Height, Width];

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                result[i, j] = new Location(
                    spreadsheet: Spreadsheet, 
                    column: From.Column + j, 
                    row: From.Row + i
                );
            }
        }

        return result;
    }

    public List<List<Location>> ToList()
    {
        List<List<Location>> result = new List<List<Location>>(Height);
        
        for (int i = 0; i < Height; i++)
        {
            result.Add(new List<Location>(Width));
            for (int j = 0; j < Width; j++)
            {
                result[i].Add(new Location(
                    spreadsheet: Spreadsheet, 
                    column: From.Column + j, 
                    row: From.Row + i
                ));
            }
        }
        
        return result;
    }

    public Location At(int dx, int dy)
    {
        if (dx <= 0 ||  dx > To.Column - From.Column)
        {
            throw new ArgumentOutOfRangeException(nameof(dx));
        }

        if (dy <= 0 || dy > To.Row - From.Row)
        {
            throw new ArgumentOutOfRangeException(nameof(dy));
        }
        
        return new Location
        {
            Column = From.Column + dx,
            Row = From.Row + dy,
            Spreadsheet = From.Spreadsheet,
        };
    }

    public bool Contains(Reference reference)
    {
        return reference switch
        {
            Range range => Contains(range),
            Location location => Contains(location),
            _ => false
        };
    }

    public bool Contains(Range range)
    {
        if (range.Spreadsheet != Spreadsheet)
        {
            return false;
        }
        
        return From.Column <= range.From.Column 
            && From.Row <= range.From.Row
            && To.Column >= range.To.Column
            && To.Row >= range.To.Row;
    }

    private bool Overlaps(Range range)
    {
        if (range.Spreadsheet != Spreadsheet)
        {
            return false;
        }
        
        // Adapted from: https://www.geeksforgeeks.org/find-two-rectangles-overlap/
        return !(From.Column > range.To.Column 
                 || range.From.Column > To.Column 
                 || To.Row > range.From.Row
                 || range.To.Row > From.Row);
    }

    public bool Contains(Location location)
    {
        if (location.Spreadsheet != From.Spreadsheet)
            return false;
        
        return location.Row >= From.Row && location.Row <= To.Row && location.Column >= From.Column && location.Column <= To.Column;
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

    public IEnumerator<Location> GetEnumerator()
    {
        // Generate all locations in the range
        for (int row = From.Row; row <= To.Row; row++)
        {
            for (int col = From.Column; col <= To.Column; col++)
            {
                yield return new Location
                {
                    Column = new AxisPosition(col),
                    Row = new AxisPosition(row),
                    Spreadsheet = From.Spreadsheet,
                };
            }
        }
    }

    public override string ToString()
    {
        return $"{Spreadsheet}!{From.ToA1()}:{To.ToA1()}";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}