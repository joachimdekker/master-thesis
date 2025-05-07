using System.Text.RegularExpressions;
using ExcelCompiler.Domain.Structure;

namespace ExcelCompiler.Representations.Structure;

/// <summary>
/// Represents a location in a spreadsheet in R1C1 format.
/// </summary>
public partial record Location : Reference
{
    public string? Spreadsheet { get; set; }
    public int Row { get; init; }
    public int Column { get; init; }

    [GeneratedRegex(@"^(?:(?:'(?<sheet>[^']+)'|(?<sheet>[^'!\s]+))!)?(?:\$?(?<col>[A-Za-z]{1,3})\$?(?<row>[0-9]{1,7}))$")]
    static partial Regex A1FormatRegex { get; }
    
    public string ToA1()
    {
        // Convert the column number to letters
        var columnLetters = string.Empty;
        var column = Column;
        while (column > 0)
        {
            column--;
            columnLetters = (char)('A' + (column % 26)) + columnLetters;
            column /= 26;
        }

        return $"{columnLetters}{Row}";
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
            Column = column,
            Row = row,
            Spreadsheet = extractedSpreadsheet ?? spreadsheet,
        };
    }

    private static int CalculateColumn(ReadOnlySpan<char> column)
    {
        if (column.Length == 0) return 0; 
        return CalculateColumn(column[..^1]) * 26 + (column[^1] - 'A' + 1);
    }
}