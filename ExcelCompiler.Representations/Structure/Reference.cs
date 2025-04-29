using Irony.Parsing;

namespace ExcelCompiler.Domain.Structure;

using XLParser;

public abstract record Reference
{
    public bool IsSingleReference => this is Location;

    public static Reference Parse(string reference, Workbook? workbook = null, string? spreadsheet = null)
    {
        var tree = XLParser.ExcelFormulaParser.ParseToTree(reference);

        if (tree.Status != ParseTreeStatus.Parsed)
        {
            throw new Exception("Could not parse reference.");
        }
        
        // Get the references
        var references = tree.Root.GetParserReferences(); // 'Sheet 2'!A2:B2
        
        // Should be only one reference
        var parsedReference = references.Single();

        return parsedReference.ReferenceType switch
        {
            ReferenceType.Cell => Location.FromA1(parsedReference.MaxLocation, spreadsheet ?? parsedReference.Worksheet),
            ReferenceType.CellRange => Range.FromString(parsedReference.LocationString, spreadsheet ?? parsedReference.Worksheet),
            ReferenceType.Table => new TableReference()
            {
                TableName = parsedReference.Name,
                ColumnName = parsedReference.TableColumns.Single()
            },
            _ => throw new Exception("Unknown reference type.")
        };
    }

    private static Spreadsheet? GetSpreadsheet(string parsedReferenceWorksheet, Workbook? workbook)
    {
        return workbook?.Spreadsheets.FirstOrDefault(s => s.Name == parsedReferenceWorksheet);
    }
}