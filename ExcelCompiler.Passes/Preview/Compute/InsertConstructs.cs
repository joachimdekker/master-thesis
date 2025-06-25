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
        foreach (var column in computeChain.Columns)
        {
            (string name, LineSelection selection) = chain.Columns.Single(c => c.Key == column.Name);

            foreach (var cellLocation in selection.Range)
            {
                ComputeUnit cell = grid[cellLocation];

                grid[cellLocation] = column switch
                {
                    DataChainColumn => cell switch
                    {
                        Function f => f,
                        _ => new DataReference(cellLocation),
                    },
                    ComputedChainColumn cc => cc.Computation!,
                    RecursiveChainColumn rc => rc.Computation!,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    private Representations.Compute.Specialized.Chain ConvertChain(ComputeGrid grid, Chain chain)
    {
        List<ChainColumn> columns = new();

        var chainComputationConverter = new ChainComputationConverter(chain);
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
                    Location = column.Range,
                };

                columns.Add(dataColumn);
                continue;
            }

            // Compute the computed column
            var unit = grid[firstCell.Location];
            var computation = chainComputationConverter.Transform(unit);

            if (IsRecursiveComputation(grid, computation))
            {
                // Recursive
                var initializationCell = chain.Initialisation.Columns.SingleOrDefault(c => c[0].Location.Column == column[0].Location.Column)?.SingleOrDefault(c => c is not EmptyCell);

                ComputeUnit? initialization = null;
                if (initializationCell is null)
                {
                    // Right now, we only support a single initialization
                    // TODO: Support multiple initialization
                    grid.TryGetValue(initializationCell.Location, out initialization);
                }

                var recursiveColumn = new RecursiveChainColumn()
                {
                    Name = name,
                    Type = firstCell.Type,
                    Computation = computation,
                    Initialization = initialization,
                    Location = column.Range,
                };
                columns.Add(recursiveColumn);
                continue;
            }

            var computedColumn = new ComputedChainColumn()
            {
                Name = name,
                Type = firstCell.Type,
                Computation = computation,
                Location = column.Range,
            };
            columns.Add(computedColumn);
        }

        return new(chain.Location)
        {
            Name = chain.Name,
            Columns = columns,
            Data = new DataReference(chain.Location.From)
        };
    }

    private bool IsRecursiveComputation(ComputeGrid grid, ComputeUnit computation)
    {
        if (computation.HasType<RecursiveChainColumn.RecursiveCellReference>()) return true;

        // Get all the references in the computation
        IEnumerable<ComputedChainColumn.CellReference> references =
            computation.GetByType<ComputedChainColumn.CellReference>();

        // Check if any of them are recursive
        return references.Any(r => IsRecursiveComputation(grid, grid[r.Location]));
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
                Computation = computation,
                Location = col.Range,
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
        var tableCellReference =  new TableColumn.CellReference(Table.Name, columnName, location);

        tableCellReference.AddDependencies(dependencies);
        return tableCellReference;
    }
}

file record ChainComputationConverter(Chain Chain) : UnitComputeGraphTransformer
{
    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        if (Chain.Columns.All(kv => !kv.Value.Range.Contains(cellReference.Reference))) return cellReference with { Dependencies = dependencies.ToList() };

        // Get the column
        string columnName = Chain.Columns.Single(kv => kv.Value.Range.Contains(cellReference.Reference)).Key;

        var location = cellReference.Location;
        var reference = cellReference.Reference;

        // If the reference is in a different row, then it is
        if (location.Row != reference.Row)
        {
            return new RecursiveChainColumn.RecursiveCellReference(columnName, Math.Abs(location.Row - reference.Row), location)
            {
                Dependencies = dependencies.ToList()
            };
        }

        // Create the reference
        return new ComputedChainColumn.CellReference(columnName, location)
        {
            Dependencies = dependencies.ToList()
        };
    }
}
