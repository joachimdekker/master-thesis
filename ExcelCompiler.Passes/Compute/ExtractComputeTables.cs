using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using ComputeTable = ExcelCompiler.Representations.Compute.Specialized.Table;
using Table = ExcelCompiler.Representations.Structure.Table;

namespace ExcelCompiler.Passes.Compute;

/// <summary>
/// Compilation pass for extracting 'compute tables'.
/// </summary>
/// <remarks>
/// <para>
/// The compute tables are tables with full data columns and column rows. This compilation pass recognizes these tables,
/// and replaces the references with these special structures.
/// </para>
/// </remarks>
[CompilerPass]
public class ExtractComputeTables
{
    public List<ComputeTable> Transform(Workbook workbook, Dictionary<Location, ComputeUnit> units)
    {
        // Look at the tables of the workbook to see if there are 'good' tables

        List<ComputeTable> tables = [];
        foreach (var table in workbook.Constructs.OfType<Table>())
        {
            ComputeTable comTable = ConvertToComputeTable(table, workbook, units);
            tables.Add(comTable);
        }

        return tables;
    }

    private ComputeTable ConvertToComputeTable(Table table, Workbook workbook, Dictionary<Location, ComputeUnit> units)
    {
        List<TableColumn> columns = new();

        var converter = new TableComputationConverter(table);
        foreach (var (name, col) in table.Columns)
        {
            var range = col.Range;
            Cell firstCell = workbook[range.From];

            // Skip columns that are not used.
            if (!units.TryGetValue(range.From, out var unit)) continue;

            TableColumn.TableColumnType type = firstCell is FormulaCell
                ? TableColumn.TableColumnType.Computed
                : TableColumn.TableColumnType.Data;

            // If the column is computed, we need to convert it to an ARM
            ComputeUnit? computation = type is TableColumn.TableColumnType.Computed
                ? converter.Transform(unit)
                : null;

            TableColumn column = new TableColumn()
            {
                Name = name,
                ColumnType = type,
                Type = firstCell.Type,
                Computation = computation
            };

            columns.Add(column);
        }

        return new ComputeTable(table.Name, table.Location, new DataReference(table.Location.From))
        {
            Columns = columns
        };
    }
}

file record TableComputationConverter(Table Table) : UnitComputeGraphTransformer
{
    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        var location = cellReference.Location;
        var reference = cellReference.Reference;

        if (Table.Columns.All(kv => !kv.Value.Range.Contains(reference))) return new CellReference(location, reference);

        // Get the column
        string columnName = Table.Columns.Single(kv => kv.Value.Range.Contains(reference)).Key;

        // Create the reference
        var tableCellReference =  new TableColumn.CellReference(Table.Name, columnName, location)
        {
            Dependencies = dependencies.ToList(),
        };

        return tableCellReference;
    }

}
