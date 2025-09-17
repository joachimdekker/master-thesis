using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using ExcelCompiler.Representations.Structure.Formulas;
using Microsoft.Extensions.Logging;
using Range = ExcelCompiler.Representations.References.Range;
using Reference = ExcelCompiler.Representations.References.Reference;
using TableReference = ExcelCompiler.Representations.Structure.Formulas.TableReference;

namespace ExcelCompiler.Passes.Structure;

[CompilerPass]
public class DetectChains(ILogger<DetectChains> logger) : DetectTables
{
    private readonly ILogger<DetectChains> _logger = logger;

    public new Chain Convert(Spreadsheet spreadsheet, Area area)
    {
        var dataPart = ExtractChainData(spreadsheet, area, out string? title, out List<Cell>? header);

        var columns = ExtractColumnHeaders(header, dataPart.Data.ColumnCount);
        var columnRanges = dataPart.Data.Columns;
        
        return new Chain
        {
            Name = title ?? area.Range.ToString().Replace("!","").Replace(":",""),
            Initialisation = dataPart.Initialisation,
            Location = area.Range,
            Columns = columns.Zip(columnRanges, (name, range) => (name, (LineSelection)range)).ToDictionary()
        };
    }

    public (Selection Initialisation, Selection Data) ExtractChainData(Spreadsheet spreadsheet, Area area,
        out string? title, out List<Cell>? header)
    {
        var dataPart = ExtractTableData(spreadsheet, area, out title, out header, out _);
        
        // Split the data part into the initialisation and the data part
        var rows = dataPart.Rows.Select(CalcHeuristic).ToList();
        
        // Get the index when the data is changing, starting from the back of the array
        for (int i = rows.Count - 1; i > 0; i--)
        {
            if (rows[i] != rows[i - 1])
            {
                return (
                    dataPart.GetRows(new System.Range(0, i)), 
                        dataPart.GetRows(System.Range.StartAt(i))
                    );
            }
        }
        
        // If we reach this point, and the data is not changing then it is not a chain and something went wrong.
        return (Selection.Empty, dataPart);
            
        int CalcHeuristic(List<Cell> row) =>
            row
                .OfType<FormulaCell>()
                .SelectMany(c => c.Formula.GetReferences())
                .Count(r => area.Range.Contains(r));
    }
    
    public bool IsChain(Spreadsheet spreadsheet, Area area)
    {
        // Now the difficult part, we need to check if the rows are all the same 'type'.
        var dataPart = ExtractChainData(spreadsheet, area, out _, out _);

        if (dataPart.Data.RowCount <= 1) return false;

        // A chain MUST have an initialisation
        if (dataPart.Initialisation.RowCount == 0) return false;
        
        // In order to beat false positives, the data needs to be longer than 1 row.
        if (dataPart.Initialisation.RowCount == 1 && dataPart.Data.RowCount == 1) return false;
        
        // Columns are the same
        foreach(List<Cell> column in dataPart.Data.Columns) {
            // Only check if the type is not computed
            if (column[0] is FormulaCell)
            {
                if (!IsComputedColumn(column, area)) return false;
                continue;
            }
            
            // Get the type of the cell
            // Replace with logic that extracts the type from the first available one.
            Type type = GetFirstType(column);
            
            // Check if columns are the same.
            if (column.Any(c => c.Type != type)) return false;
        }

        return true;
    }

    private Type GetFirstType(IEnumerable<Cell> column)
    {
        foreach (Cell cell in column)
        {
            if (cell is not EmptyCell) return cell.Type;
        }
        
        throw new Exception("No type was found; Empty column");
    }

    protected new bool IsComputedColumn(List<Cell> column, Area area)
    {
        // First check if all cells are formula cells
        if (column.Any(c => c is not FormulaCell)) return false;
        
        FormulaCell[] formulaCells = column.Cast<FormulaCell>().ToArray();
        
        // Transform every column formula to reference columns instead of cells
        FormulaExpression[] transformedCells = formulaCells.Select(c => NormalizeFormula(c.Formula, c.Location, area.Range)).ToArray();

        // Check if the transformed cells are all the same
        return transformedCells.All(c => TransformEqual(c ,transformedCells[0]));
    }
    
    protected new bool TransformEqual(FormulaExpression left, FormulaExpression right)
    {
        return (left, right) switch
        {
            (Constant cl, Constant cr) => cl.Value == cr.Value,
            (Function fl, Function fr) => fl.Name == fr.Name &&
                                          fl.Arguments.Zip(fr.Arguments, TransformEqual).All(t => t),
            (RangeReference rrl, RangeReference rrr) => rrl.Reference.Equals(rrr.Reference),
            (TableReference trl, TableReference trr) => trl.Reference.TableName == trr.Reference.TableName &&
                                                        trl.Reference.ColumnNames.SequenceEqual(trr.Reference
                                                            .ColumnNames),
            (RelativeFormulaTransformer.RelativeRange rlrl, RelativeFormulaTransformer.RelativeRange rlrr) 
                => rlrl.Members.Zip(rlrr.Members, TransformEqual).All(t => t),
            (RelativeFormulaTransformer.RelativeReference rlr, RelativeFormulaTransformer.RelativeReference rll)
                => rlr.ColumnOffset == rll.ColumnOffset && rlr.RowOffset == rll.RowOffset,
            (CellReference crl, CellReference crr) => crl.Reference.Equals(crr.Reference),
            _ => false
        };
    }

    private FormulaExpression NormalizeFormula(FormulaExpression expression, Location location, Range range)
    {
        return new RelativeFormulaTransformer(location, range).Traverse(expression);
    }
}


file record RelativeFormulaTransformer(Location Location, Range Range) : FormulaTransformer
{
    // ReSharper disable NotAccessedPositionalProperty.Local
    public record RelativeReference(int ColumnOffset, int RowOffset) : FormulaExpression;
    public record RelativeRange(List<RelativeReference> Members) : FormulaExpression;
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
        List<RelativeReference> references = range.Reference
            .Select(l => (RowOffset: Location.Row - l.Row, ColumnOffset: Location.Column - l.Column))
            .Select(i => new RelativeReference(i.ColumnOffset, i.RowOffset))
            .ToList();

        return new RelativeRange(references);
    }
}