using System.Text.RegularExpressions;

namespace ExcelCompiler.Representations.References;

/// <summary>
/// Represents a location in a spreadsheet in R1C1 format.
/// </summary>
public partial record Location : Reference, IComparable<Location>
{
    public Location()
    {
    }

    public Location(string? spreadsheet, int column, int row)
        : this(spreadsheet, new AxisPosition(column), new AxisPosition(row))
    { }

    public Location(string? spreadsheet, AxisPosition column, AxisPosition row)
    {
        Spreadsheet = spreadsheet;
        Column = column;
        Row = row;
    }

    public string? Spreadsheet { get; set; }
    
    public AxisPosition Row { get; init; }
    public AxisPosition Column { get; init; }

    [GeneratedRegex(@"^(?:(?:'(?<sheet>[^']+)'|(?<sheet>[^'!\s]+))!)?(?:\$?(?<col>[A-Za-z]{1,3})\$?(?<row>[0-9]{1,7}))$")]
    static partial Regex A1FormatRegex { get; }
    
    public string ToA1()
    {
        // Convert the column number to letters
        var columnLetters = string.Empty;
        int column = Column;
        while (column > 0)
        {
            column--;
            columnLetters = (char)('A' + (column % 26)) + columnLetters;
            column /= 26;
        }

        return $"{columnLetters}{(int)Row}";
    }
    
    public static Location FromA1(string a1Format, string? spreadsheet = null)
    {
        // Get the numbers out of the string
        var match = A1FormatRegex.Match(a1Format);
        var extractedSpreadsheet = match.Groups["sheet"].Success ? match.Groups["sheet"].Value : null;
        var extractedColumn = match.Groups["col"].Value;
        var extractedRow = match.Groups["row"].Value;
        
        // Convert to R1C1 format
        var column = CalculateColumn(extractedColumn.AsSpan());
        var row = int.Parse(extractedRow);

        return new Location
        {
            Column = new AxisPosition(column),
            Row = new AxisPosition(row),
            Spreadsheet = extractedSpreadsheet ?? spreadsheet,
        };
    }

    private static int CalculateColumn(ReadOnlySpan<char> column)
    {
        if (column.Length == 0) return 0; 
        return CalculateColumn(column[..^1]) * 26 + (column[^1] - 'A' + 1);
    }

    public int CompareTo(Location? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var spreadsheetComparison = string.Compare(Spreadsheet, other.Spreadsheet, StringComparison.Ordinal);
        if (spreadsheetComparison != 0) return spreadsheetComparison;
        var rowComparison = Row.CompareTo(other.Row);
        return rowComparison != 0 ? rowComparison : Column.CompareTo(other.Column);
    }

    public static Location Max(params Location[] locations)
    {
        return locations.OrderDescending().First();
    }
    
    public static Location Min(params Location[] locations)
    {
        return locations.Order().First();
    }
}