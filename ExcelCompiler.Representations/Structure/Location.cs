using System.Text.RegularExpressions;

namespace ExcelCompiler.Domain.Structure;

/// <summary>
/// Represents a location in a spreadsheet in R1C1 format.
/// </summary>
public partial record Location : Reference
{
    public Spreadsheet? Spreadsheet { get; init; }
    public int Row { get; init; }
    public int Column { get; init; }

    [GeneratedRegex("^(?<column>^[A-Z]+)(?<row>[1-9][0-9]*)$")]
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
    
    public static Location FromA1(string a1Format, Spreadsheet? spreadsheet = null)
    {
        // Get the numbers out of the string
        var match = A1FormatRegex.Match(a1Format);
        var extractedColumn = match.Groups["column"].Value;
        var extractedRow = match.Groups["row"].Value;
        
        // Convert to R1C1 format
        var column = CalculateColumn(extractedColumn.AsSpan());
        var row = int.Parse(extractedRow);

        return new Location
        {
            Column = column,
            Row = row,
            Spreadsheet = spreadsheet
        };
    }

    private static int CalculateColumn(ReadOnlySpan<char> column)
    {
        if (column.Length == 0) return 0; 
        return CalculateColumn(column[..^1]) * 26 + (column[^1] - 'A' + 1);
    }
}