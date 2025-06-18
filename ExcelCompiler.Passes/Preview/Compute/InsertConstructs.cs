using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Chain = ExcelCompiler.Representations.Structure.Chain;
using StructureConstruct = ExcelCompiler.Representations.Structure.Construct;
using Table = ExcelCompiler.Representations.Structure.Table;
using ComputeTable = ExcelCompiler.Representations.Compute.Specialized.Table;

namespace ExcelCompiler.Passes.Preview.Compute;

/// <summary>
/// Inserts the constructs into the workbook and transforms them to the Compute Unit IR.
/// </summary>
[CompilerPass]
public class InsertConstructs
{
    public ComputeGrid Generate(ComputeGrid grid, List<StructureConstruct> constructs)
    {
        // The Compute Grid was a better way to do this, but now I am doubting myself.
        // Ultimately, we would want to be able to edit the references.
        // However, we need the graph for that.
        
        // What we could always do, is just edit the references so that they correspond to the data references.
        // And for the computed columns, we can use a special node
        // Then, when we partially reference the table, we can use indices.
        // However, that is only supported in in-memory mode.
        
        // This is good stuff for the discussion though
        
        // Type for type, add the constructs to the grid.
        // Because of how the areas are detected, we can guarantee that no construct is overlapping.

        foreach (Table table in constructs.OfType<Table>())
        {
            Representations.Compute.Specialized.Table computeTable = ConvertTable(grid, table);
            
            // Embed the table into the grid
            EmbedTable(grid, table, computeTable);
            
            // Add the table to the list of tables
            grid.Structures.Add(computeTable);
        }

        foreach (Chain chain in constructs.OfType<Chain>())
        {
            Representations.Compute.Specialized.Chain computeChain = ConvertChain(grid, chain);
            
            EmbedChain(grid, chain, computeChain);
            
            grid.Structures.Add(computeChain);
        }

        return grid;
    }

    private void EmbedChain(ComputeGrid grid, Chain chain, Representations.Compute.Specialized.Chain computeChain)
    {
        throw new NotImplementedException();
    }

    private Representations.Compute.Specialized.Chain ConvertChain(ComputeGrid grid, Chain chain)
    {
        List<ChainColumn> columns = new();

        foreach (var (name, column) in chain.Columns)
        {
            // Skip columns that are not used.
            if (!column.TryGetFirstNonEmptyCell(out var firstCell)) continue;
            
            // Check which version of the column is used
            if (column.Any(c => c is not FormulaCell and not EmptyCell))
            {
                // Data
                var dataColumn = new DataChainColumn()
                {
                    Name = name,
                    Type = firstCell.Type,
                };

                columns.Add(dataColumn);
                continue;
            }

            if (IsRecursiveColumn(column, chain))
            {
                
            }
        }

        return new(chain.Location)
        {
            Name = chain.Name,
            Columns = columns,
            Data = new DataReference(chain.Location.From)
        };
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
                
                grid[column.Range.From] = computedColumn.ColumnType switch
                {
                    TableColumn.TableColumnType.Computed => computedColumn.Computation!,
                    TableColumn.TableColumnType.Data => cell switch
                    {
                        Function f => f,
                        _ => new DataReference(cellLocation),
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
    
    private ComputeTable ConvertTable(ComputeGrid grid, Table table)
    {
        List<TableColumn> columns = new();
        
        var converter = new TableComputationConverter(table);
        foreach (var (name, col) in table.Columns)
        {
            // Skip columns that are not used.
            if (!col.TryGetFirstNonEmptyCell(out var firstCell)) continue;
            
            TableColumn.TableColumnType type = firstCell is FormulaCell
                ? TableColumn.TableColumnType.Computed
                : TableColumn.TableColumnType.Data;
            
            var unit = grid[firstCell.Location];

            // If the column is computed, we need to convert it to an ARM
            ComputeUnit? computation = type is TableColumn.TableColumnType.Computed
                ? converter.Transform(unit)
                : null;

            TableColumn column = new TableColumn
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

file record TableComputationConverter(Table Table) : UnitSupportGraphTransformer
{
    protected override ComputeUnit CellReference(Location location, IEnumerable<ComputeUnit> dependencies, Location reference)
    {
        if (Table.Columns.All(kv => !kv.Value.Range.Contains(reference))) return new CellReference(location, reference);
        
        // Get the column
        string columnName = Table.Columns.Single(kv => kv.Value.Range.Contains(reference)).Key;
                
        // Create the reference
        var cellReference =  new TableColumn.CellReference(Table.Name, columnName, location);
        
        cellReference.AddDependencies(dependencies);
        return cellReference;
    }
}

file record ChainComputationConverter(Chain Chain) : UnitSupportGraphTransformer
{
    protected override ComputeUnit CellReference(Location location, IEnumerable<ComputeUnit> dependencies, Location reference)
    {
        if (Chain.Columns.All(kv => !kv.Value.Range.Contains(reference))) return new CellReference(location, reference);
        
        // Get the column
        string columnName = Chain.Columns.Single(kv => kv.Value.Range.Contains(reference)).Key;
                
        // Based on the current location, and the 
        
    }
}