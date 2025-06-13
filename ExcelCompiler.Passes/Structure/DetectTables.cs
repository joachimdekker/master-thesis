using System.Diagnostics.CodeAnalysis;
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
        var tableData = ExtractTableData(spreadsheet, area, out string? title, out List<Cell>? header);
        
        // Get the columns
        bool hasHeader = header.All(c => c is ValueCell<string>);
        var columns = ExtractColumnHeaders(header, tableData.ColumnCount);
        var columnRanges = tableData.Columns;

        var excelTable = spreadsheet.Tables.SingleOrDefault(t => area.Range.Contains(t.Location));
        
        return new Table
        {  
            Name = excelTable?.Name ?? (title ?? area.Range.ToString().Replace("!","").Replace(":","")),
            Location = area.Range,
            Header = hasHeader ? (Selection)header : null,
            Data = tableData,
            Columns = columns.Zip(columnRanges, (name, range) => (name, (Selection)range)).ToDictionary()
        };
    }

    protected static List<string> ExtractColumnHeaders(List<Cell>? header, int numHeaders)
    {
        if (header is not null && header.All(c => c is ValueCell<string> or EmptyCell))
        {
            return header.Select((h, i) => h is not ValueCell<string> v ? $"Column {i}" : v.Value ).ToList();
        }

        // If there is no header, we need to generate a column name.
        return Enumerable.Range(0, header?.Count ?? numHeaders).Select(i => $"Column {i}").ToList();
    }

    public bool IsTable(Spreadsheet spreadsheet, Area area)
    {
        // Check if the table has an header
        // A header is classified if the first row of the area contains all text.
        var dataPart = ExtractTableData(spreadsheet, area, out _, out _);
        
        // Columns are the same
        int noColumns = dataPart.ColumnCount;
        for (int i = 0; i < noColumns; i++)
        { 
            List<Cell> column = dataPart.GetColumn(i);
            
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

    protected static Selection ExtractTableData(Spreadsheet spreadsheet, Area area, out string? title, out List<Cell>? header)
    {
        // The table may have a header
        var tableCells = spreadsheet[area.Range];
        
        // The table may have a title.
        bool hasTitle = TryGetTitle(tableCells, out title);
        bool hasHeader = TryGetHeader(tableCells, hasTitle, out header);

        int startData = (hasTitle ? 1 : 0) + (hasHeader ? 1 : 0);
        
        var dataPart = tableCells.GetRows(System.Range.StartAt(hasHeader ? 1 : 0));
        return dataPart;
    }

    protected static bool TryGetHeader(Selection tableCells, bool hasTitle, [NotNullWhen(true)] out List<Cell>? header)
    {
        // Check which row we need to check
        int rowToCheck = hasTitle ? 1 : 0;
        
        // Check if the first row is a header
        header = tableCells.GetRow(rowToCheck);
        
        // Check if the first row has exactly one value string cell and the rest are empty cells
        if (header.All(c => c is ValueCell<string> or EmptyCell))
        {
            return true;
        }

        header = null!;
        return false;
    }

    protected static bool TryGetTitle(Selection selection, [NotNullWhen(true)] out string? title)
    {
        // Check if the first row is a title
        List<Cell> firstRow = selection.GetRow(0);
        
        // Check if the first row has exactly one value string cell and the rest are empty cells
        if (firstRow.Count(c => c is ValueCell<string>) != 1 || firstRow.Count(c => c is EmptyCell) != firstRow.Count - 1)
        {
            title = null;
            return false;
        }
        
        title = (firstRow.Single(c => c is ValueCell<string>) as ValueCell<string>)!.Value;
        return true;
    }

    protected bool IsComputedColumn(List<Cell> column, Area area)
    {
        // First check if all cells are formula cells
        if (column.Any(c => c is not FormulaCell)) return false;
        
        FormulaCell[] formulaCells = column.Cast<FormulaCell>().ToArray();
        
        // Check if the formula has a formula that refers to the table
        if (!formulaCells.Any(c => c.Formula.GetReferences().Any(r => area.Range.Contains(r)))) return false;
        
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