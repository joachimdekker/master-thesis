using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using ExcelCompiler.Representations.Structure.Formulas;
using Range = ExcelCompiler.Representations.References.Range;
using Reference = ExcelCompiler.Representations.References.Reference;
using TableReference = ExcelCompiler.Representations.Structure.Formulas.TableReference;

namespace ExcelCompiler.Passes.Structure;

[CompilerPass]
public class DetectTables
{
    /// <summary>
    /// Convert the given range of cells to a table.
    /// </summary>
    /// <remarks>
    /// This function should be called after checking if the area is a table
    /// using <see cref="IsTable(Spreadsheet, Area)"/>.
    /// </remarks>
    /// <param name="spreadsheet"></param>
    /// <param name="area"></param>
    /// <returns></returns>
    public Table Convert(Spreadsheet spreadsheet, Area area)
    {
        // Get the column names
        var tableCells = spreadsheet[area.Range];
        var header = tableCells.GetRow(0);
        
        // Get the columns
        bool hasHeader = header.All(c => c is ValueCell<string>);
        int rowCount = hasHeader ? tableCells.GetLength(0) - 1 : tableCells.GetLength(0);
        var columns = ExtractColumnHeaders(header);
        var columnRanges = header.Select((c) =>
        {
            Location start = !hasHeader ? c.Location : c.Location with { Row = c.Location.Row + 1 };
            Location end = c.Location with { Row = c.Location.Row + rowCount };
            return new Range(start, end);
        });

        return new Table()
        {  
            Name = area.Range.ToString(),
            Location = area.Range,
            Columns = columns.Zip(columnRanges, (name, range) => (name, range)).ToDictionary()
        };
    }

    protected static List<string> ExtractColumnHeaders(Cell[] header)
    {
        if (header.All(c => c is ValueCell<string> or EmptyCell))
        {
            return header.Select((h, i) => h is not ValueCell<string> v ? $"Column {i}" : v.Value ).ToList();
        }

        // If there is no header, we need to generate a column name.
        return Enumerable.Range(0, header.Length).Select(i => $"Column {i}").ToList();
    }

    public bool IsTable(Spreadsheet spreadsheet, Area area)
    {
        // Check if the table has an header
        // A header is classified if the first row of the area contains all text.
        var dataPart = ExtractTableData(spreadsheet, area, out _);

        // Columns are the same
        int noColumns = dataPart.GetLength(1);
        for (int i = 0; i < noColumns; i++)
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
            Type cellType = column[0].GetType();
            
            // Check if columns are the same.
            if (column.Where(c => c is not EmptyCell).Any(c => c.GetType() != cellType || c.Type != type)) return false;
        }

        return true;
    }

    protected static Cell[,] ExtractTableData(Spreadsheet spreadsheet, Area area, out Cell[]? header)
    {
        var tableCells = spreadsheet[area.Range];
        
        // Check the types of the first cells
        header = tableCells.GetRow(0);
        bool hasHeader = header.All(c => c is ValueCell<string> or EmptyCell);

        if (!hasHeader) header = null;
        
        var dataPart = tableCells.Copy(selectedRows: (hasHeader ? 1 : 0, tableCells.GetLength(0)));
        return dataPart;
    }

    protected bool IsComputedColumn(Cell[] column, Area area)
    {
        // First check if all cells are formula cells
        if (column.Any(c => c is not FormulaCell)) return false;
        
        FormulaCell[] formulaCells = column.Cast<FormulaCell>().ToArray();
        
        // Then, check if any references inside the FormulaCell are inside the area
        // If they are, check if they fall in the same row.
        if (formulaCells.Any(c => !IsFormulaNonDependent(c, area.Range))) return false;
        
        // Transform every column formula to reference columns instead of cells
        FormulaExpression[] transformedCells = formulaCells.Select(c => TransformFormula(c, area.Range)).ToArray();
        
        // Check if the transformed cells are all the same
        if (transformedCells.Any(c => !TransformEqual(c ,transformedCells[0]))) return false;
        
        return true;
    }

    protected bool TransformEqual(FormulaExpression left, FormulaExpression right)
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
            (CellReference crl, CellReference crr) => crl.Reference.Equals(crr.Reference),
            _ => false
        };
    }

    protected bool IsFormulaNonDependent(FormulaCell cell, Range range)
    {
        List<Reference> references = cell.Formula.GetReferences();

        // All cells that are within the range should be from the same row
        return references
            .Where(range.Contains)
            .All(r => r is Location l && l.Row == cell.Location.Row
            || (r is Range ra && ra.From.Row == cell.Location.Row && ra.To.Row == cell.Location.Row));
    }

    protected FormulaExpression TransformFormula(FormulaCell cell, Range range)
    {
        return new FormulaReferenceTransformer(range).Traverse(cell.Formula);
    }
}

file record FormulaReferenceTransformer(Range Range) : FormulaTransformer
{
    protected override FormulaExpression CellReference(CellReference reference)
    {
        if (!Range.Contains(reference.Reference)) return reference;
        
        // If the range contains the reference, we know that it should be in the same row,
        // so we need to get the row and convert it to a table reference.
        
        // Get the offset of the range and just call it the offset for now.
        int offset = Range.From.Column - reference.Reference.Column;

        return new TableReference(new Representations.References.TableReference()
        {
            ColumnNames = [$"Column {offset}"],
            TableName = "Temp"
        });
    }

    protected override FormulaExpression RangeReference(RangeReference range)
    {
        if (!Range.Contains(range.Reference)) return range;
        
        // This one is a bit more difficult
        // The range can be partially in the table. But then it is still a relative range.
        // We should just only accept ranges that are only in the area.
        
        // Get all the locations in the range, and transform it to a multiple column operation.
        List<string> columnNames = range.Reference.GetLocations()
            .Select(l => Range.From.Column - l.Column)
            .Select(i => $"Column {i}")
            .ToList();

        return new TableReference(new Representations.References.TableReference()
        {
            ColumnNames = columnNames,
            TableName = "Temp"
        });

    }
}