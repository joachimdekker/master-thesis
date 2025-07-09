using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using Chain = ExcelCompiler.Representations.Structure.Chain;
using Construct = ExcelCompiler.Representations.Structure.Construct;
using Table = ExcelCompiler.Representations.Structure.Table;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class InsertConstructs
{
    public ComputeGrid Transform(ComputeGrid grid, List<Construct> constructs)
    {
        foreach (var construct in grid.Structures)
        {
            var structure = constructs.Single(s => s.Name == construct.Id);

            switch (construct)
            {
                case Representations.Compute.Specialized.Table table:
                    EmbedTable(grid, structure as Table, table);
                    break;
                case Representations.Compute.Specialized.Chain chain:
                    EmbedChain(grid, structure as Chain, chain);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return grid;
    }
    
    private void EmbedChain(ComputeGrid grid, Chain chain, Representations.Compute.Specialized.Chain computeChain)
    {
        foreach (var column in computeChain.Columns)
        {
            foreach (var cellLocation in column.Location)
            {
                int index = cellLocation.Row - column.Location.From.Row;
                switch (column)
                {
                    case DataChainColumn dataColumn:
                        if (grid.TryGetValue(cellLocation, out var cell) && cell is Function) break;
                        
                        // Create a proper data reference
                        grid[cellLocation] = new DataChainColumn.Reference(
                            ChainName: chain.Name,
                            ColumnName: column.Name,
                            Index: index,
                            Location: cellLocation
                        );
                        break;
                    
                    case ComputedChainColumn cc:
                        grid[cellLocation] = new ComputedChainColumn.CellReference(
                            ChainName: chain.Name,
                            ColumnName: column.Name,
                            Index: index,
                            Location: cellLocation
                        );
                        break;
                    
                    case RecursiveChainColumn rc:
                        grid[cellLocation] = new RecursiveChainColumn.RecursiveCellReference(
                            ChainName: chain.Name,
                            ColumnName: column.Name,
                            Recursion: index,
                            Location: cellLocation
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void EmbedTable(ComputeGrid grid, Table table, Representations.Compute.Specialized.Table computeTable)
    {
        // For the whole grid,
        foreach (var (name, column) in table.Columns)
        {
            // Check if the column is computed or not
            TableColumn? computedColumn = computeTable.Columns.SingleOrDefault(c => c.Name == name);

            if (computedColumn is null) continue;

            // Change the reference to a data or TableComputation node
            foreach (var cellLocation in column.Range)
            {
                ComputeUnit cell = grid[cellLocation];

                int index = cellLocation.Row - column.Range.From.Row;
                grid[cellLocation] = computedColumn.ColumnType switch
                {
                    TableColumn.TableColumnType.Computed => computedColumn.Computation!,
                    TableColumn.TableColumnType.Data => cell switch
                    {
                        Function f => f,
                        _ => new TableColumn.CellReference(table.Name, name, index, cellLocation) { Type = cell.Type},
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}