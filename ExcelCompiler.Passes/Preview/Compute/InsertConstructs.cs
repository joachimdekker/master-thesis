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

    private Representations.Compute.Specialized.Chain ConvertChain(ComputeGrid grid, Chain chain)
    {
        List<ChainColumn> columns = new();

        var chainComputationConverter = new ChainComputationConverter(chain);
        foreach (var (name, column) in chain.Columns)
        {
            // Skip columns that are not used.
            if (!column.TryGetFirstNonEmptyCell(out var firstCell) || !column.Any(c => grid.ContainsLocation(c.Location))) continue;

            // Calculate the range of the column, include the footer and init
            var range = column.Range with
            {
                From = column.Range.From with { Row = column.Range.From.Row - (chain.Initialisation?.Rows.Count ?? 0) },
                To = column.Range.To with { Row = column.Range.To.Row + (chain.Footer?.Rows.Count ?? 0) },
            };
            
            // Check which version of the column is used
            if (column.Any(c => c is not FormulaCell and not EmptyCell))
            {
                // Data
                var dataColumn = new DataChainColumn()
                {
                    Name = name,
                    Type = firstCell.Type,
                    Location = range,
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
                if (initializationCell is not null)
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
                    Location = range,
                };
                columns.Add(recursiveColumn);
                continue;
            }

            var computedColumn = new ComputedChainColumn()
            {
                Name = name,
                Type = firstCell.Type,
                Computation = computation,
                Location = range,
            };
            columns.Add(computedColumn);
        }
        
        
        Representations.Compute.Specialized.Chain computedChain = new(chain.Location)
        {
            Name = chain.Name,
            Columns = columns,
            Data = new DataReference(chain.Location.From)
        };
        
        // Update the columns once more
        foreach (var column in computedChain.Columns)
        {
            switch (column)
            {
                case RecursiveChainColumn recursiveChainColumn:
                {
                    // Fix the references
                    var referenceFixer = new ChainReferenceFixer(computedChain);
                    var fixedComputation = referenceFixer.Transform(recursiveChainColumn.Computation!);
                    recursiveChainColumn.Computation = fixedComputation;
                    break;
                }
                case ComputedChainColumn computedChainColumn:
                {
                    // Fix the references
                    var referenceFixer = new ChainReferenceFixer(computedChain);
                    var fixedComputation = referenceFixer.Transform(computedChainColumn.Computation!);
                    computedChainColumn.Computation = fixedComputation;
                    break;
                }
            }
        }
        
        return computedChain;
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
                        _ => new DataReference(cellLocation) { Type = cell.Type},
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
            if (!col.TryGetFirstNonEmptyCell(out var firstCell) || !col.Any(c => grid.ContainsLocation(c.Location))) continue;

            TableColumn.TableColumnType type = firstCell is FormulaCell
                ? TableColumn.TableColumnType.Computed
                : TableColumn.TableColumnType.Data;
            
            // If the column is computed, we need to convert it to an ARM
            ComputeUnit? computation = type is TableColumn.TableColumnType.Computed
                ? converter.Transform(grid[firstCell.Location])
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
        var tableCellReference = new TableColumn.CellReference(Table.Name, columnName, location)
        {
            Dependencies = dependencies.ToList(),
        };
        
        return tableCellReference;
    }
}

file record ChainComputationConverter(Chain Chain) : UnitComputeGraphTransformer
{
    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        if (!Chain.Location.Contains(cellReference.Reference)) return cellReference with { Note = "Outside Chain", Dependencies = dependencies.ToList() };

        // Get the column
        string columnName = Chain.Columns.Single(kv => kv.Value.Range.From.Column == cellReference.Reference.Column).Key;

        var location = cellReference.Location;
        var reference = cellReference.Reference;

        // If the reference is in a different row, then it is
        if (location.Row != reference.Row)
        {
            return new RecursiveChainColumn.RecursiveCellReference(Chain.Name, columnName, location.Row - reference.Row, location)
            {
                Dependencies = dependencies.ToList()
            };
        }
        
        // Create the reference
        // Get the index of the cell reference in comparison to the first cell
        
        return new ComputedChainColumn.CellReference(Chain.Name, columnName, location.Row - reference.Row, location)
        {
            Dependencies = dependencies.ToList()
        };
    }
}

file record ChainReferenceFixer(Representations.Compute.Specialized.Chain Chain) : UnitComputeGraphTransformer
{
    protected override ComputeUnit Other(ComputeUnit unit, IEnumerable<ComputeUnit> dependencies)
    {
        if (unit is not ComputedChainColumn.CellReference cellReference) return base.Other(unit, dependencies);
        
        // If the reference is referencing a compute column
        ChainColumn column = Chain.Columns.Single(c => c.Name == cellReference.ColumnName);

        if (column is RecursiveChainColumn recursiveChainColumn)
        {
            return new RecursiveChainColumn.RecursiveCellReference(Chain.Name, column.Name, 0, cellReference.Location)
            {
                Dependencies = new List<ComputeUnit>(dependencies),
            };
        }
        
        return base.Other(unit, dependencies);
    }
}
