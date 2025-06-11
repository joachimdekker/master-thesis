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
        foreach (var table in workbook.Tables)
        {
            var isGoodTable = true;
            int skippedColumns = 0;
            foreach (var (_, column) in table.Columns)
            {
                var columnRange = column.Range;
                Cell firstCell = workbook[columnRange.From];
             
                // Skip columns that are not used
                if (!units.TryGetValue(columnRange.From, out _))
                {
                    skippedColumns++;
                    continue;
                }
                
                if (firstCell is FormulaCell formulaCell)
                {
                    // Generate compute arm
                    ComputeUnit original = units[formulaCell.Location];
                    ComputeUnit arm = ConvertToArm(table, original);

                    List<Location> locations = columnRange.GetLocations().ToList();
                    if (locations.All(l => workbook[l] is FormulaCell)
                        && locations.All(l => ConvertToArm(table, units[l]).ComputationalEquivalent(arm))) 
                        continue;
                    
                    isGoodTable = false;
                    break;
                }

                // Then everything should be a value cell
                if (columnRange.GetLocations().All(l => workbook[l] is ValueCell or EmptyCell)) continue;
                    
                isGoodTable = false;
                break;
            }

            if (!isGoodTable || skippedColumns == table.Columns.Count) continue;
            
            ComputeTable comTable = ConvertToComputeTable(table, workbook, units);
            tables.Add(comTable);
        }

        return tables;
    }

    private ComputeUnit ConvertToArm(Table table, ComputeUnit unit)
    {
        ComputeUnit newUnit = unit switch
        {
            CellReference {Reference: { } reference} when table.Location.Contains(reference) => CreateColumnReference(reference),
            _ => unit
        };

        foreach (var dependency in unit.Dependencies.ToList())
        {
            newUnit.Dependencies.Remove(dependency);
            
            // Convert
            var newDep = ConvertToArm(table, dependency);
            newUnit.Dependencies.Add(newDep);
        }
        
        return newUnit;

        TableColumn.CellReference CreateColumnReference(Location reference)
        {
            // Get the column
            string columnName = table.Columns.Single(kv => kv.Value.Range.Contains(reference)).Key;
            
            // Create the reference
            return new(table.Name, columnName, unit.Location);
        }
    }

    private ComputeTable ConvertToComputeTable(Table table, Workbook workbook, Dictionary<Location, ComputeUnit> units)
    {
        List<TableColumn> columns = new();

        foreach (var (name, col) in table.Columns)
        {
            var range = col.Range;
            Cell firstCell = workbook[range.From];

            // Skip columns that are not used.
            if (!units.TryGetValue(range.From, out var unit)) continue;
            
            TableColumn.TableColumnType type = firstCell is FormulaCell
                ? TableColumn.TableColumnType.Computed
                : TableColumn.TableColumnType.Data;

            ComputeUnit? computation = type is TableColumn.TableColumnType.Computed
                ? ConvertToArm(table, unit)
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
        
        return new ComputeTable(table.Name, new DataReference(table.Location.From))
        {
            Columns = columns,
            Location = table.Location,
        };
    }
}