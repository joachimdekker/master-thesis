using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using ExcelCompiler.Representations.Structure.Formulas;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Passes.Structure;

[CompilerPass]
public class DetectChains : DetectTables
{
    public new Chain Convert(Spreadsheet spreadsheet, Area area)
    {
        var dataPart = ExtractTableData(spreadsheet, area, out Cell[]? header);
        
        var headerData = header ?? dataPart.GetRow(0);
        
        int rowCount = dataPart.GetLength(0);
        var columns = ExtractColumnHeaders(headerData);
        var columnRanges = headerData.Select((c) =>
        {
            Location start = header is not null ? c.Location : c.Location with { Row = c.Location.Row + 1 };
            Location end = c.Location with { Row = c.Location.Row + rowCount };
            return new Range(start, end);
        });
        
        return new Chain()
        {
            Name = area.Range.ToString(),
            Location = area.Range,
            Columns = columns.Zip(columnRanges, (name, range) => (name, range)).ToDictionary()
        };
    }
    
    public bool IsChain(Spreadsheet spreadsheet, Area area)
    {
        // Now the difficult part, we need to check if the rows are all the same 'type'.
        var dataPart = ExtractTableData(spreadsheet, area, out _);
        
        // Columns are the same
        for (int i = 0; i <= dataPart.GetLength(0); i++)
        {
            Cell[] column = dataPart.GetColumn(i);
            
            // Only check if the type is not computed
            if (column[0] is FormulaCell)
            {
                if (!IsComputedColumn(column, area)) return false;
                continue;
            }
            
            // Get the type of the cell
            // Replace with logic that extracts the type from the first available one.
            Type type = column[0].Type;
            
            // Check if columns are the same.
            if (column.Any(c => c.Type != type)) return false;
        }

        return true;
    }
    
    protected new bool IsComputedColumn(Cell[] column, Area area)
    {
        // First check if all cells are formula cells
        if (column.Any(c => c is not FormulaCell)) return false;
        
        FormulaCell[] formulaCells = column.Cast<FormulaCell>().ToArray();
        
        // Transform every column formula to reference columns instead of cells
        FormulaExpression[] transformedCells = formulaCells.Select(c => NormalizeFormula(c.Formula, c.Location, area.Range)).ToArray();

        // Check if the transformed cells are all the same
        return transformedCells.All(c => c == transformedCells[0]);
    }

    private FormulaExpression NormalizeFormula(FormulaExpression expression, Location location, Range range)
    {
        return new RelativeFormulaTransformer(location, range).Traverse(expression);
    }
}


file record RelativeFormulaTransformer(Location Location, Range Range) : FormulaTransformer
{
    // ReSharper disable NotAccessedPositionalProperty.Local
    record RelativeReference(int ColumnOffset, int RowOffset) : FormulaExpression;
    record RelativeRange(List<RelativeReference> Members) : FormulaExpression;
    // ReSharper restore NotAccessedPositionalProperty.Local
    
    protected override FormulaExpression CellReference(CellReference reference)
    {
        if (!Range.Contains(reference.Reference)) return reference;
        
        // If the range contains the reference, we know that it should be in the same row,
        // so we need to get the row and convert it to a table reference.
        
        // Get the offset of the range and just call it the offset for now.
        int columnOffset = Location.Column - reference.Reference.Column;
        int rowOffset = Location.Row - reference.Reference.Row;

        return new RelativeReference(columnOffset, rowOffset);
    }

    protected override FormulaExpression RangeReference(RangeReference range)
    {
        if (!Range.Contains(range.Reference)) return range;
        
        // This one is a bit more difficult
        // The range can be partially in the table. But then it is still a relative range.
        // We should just only accept ranges that are only in the area.
        
        // Get all the locations in the range, and transform it to a multiple column operation.
        List<RelativeReference> references = range.Reference.GetLocations()
            .Select(l => (RowOffset: Location.Row - l.Row, ColumnOffset: Location.Column - l.Column))
            .Select(i => new RelativeReference(i.ColumnOffset, i.RowOffset))
            .ToList();

        return new RelativeRange(references);
    }
}