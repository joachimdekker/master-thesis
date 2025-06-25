using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class ReplaceConstructDependencies
{
    public ComputeGraph Transform(ComputeGraph graph)
    {

    }
}

file record RecursiveTypeTransformer(List<Construct> Constructs) : UnitComputeGraphTransformer()
{
    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        // Check if the range references a table column
        var construct = Constructs.SingleOrDefault(c => c.Location.Contains(rangeReference.Location));
        if (construct is null) return rangeReference with { Dependencies = dependencies.ToList() };

        // Get the type of the range based on the type of the construct
        return construct switch
        {
            Table table when table.Columns.Any(c => c.Location.Contains(rangeReference.Location))
                => new ColumnOperation(table.Name, table.Columns.Single(c => c.Location.Contains(rangeReference.Location)).Name, rangeReference.Location),
            Chain chain => ChainRangeReference(chain, rangeReference, dependencies),
            _ => rangeReference with {Dependencies = dependencies.ToList()},
        };
    }

    private ComputeUnit ChainRangeReference(Chain chain, RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        ChainColumn? column = chain.Columns.SingleOrDefault(c => c.Location.Contains(rangeReference.Location));

        if (column is null) return rangeReference with { Dependencies = dependencies.ToList() };

        // TODO: Perhaps something to easily detect something
        return new ColumnOperation(chain.Name, column.Name, rangeReference.Location);
    }

    protected override ComputeUnit TableReference(TableReference tableReference, IEnumerable<ComputeUnit> dependencies)
    {
        return tableReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        var construct = Constructs.SingleOrDefault(c => c.Location.Contains(cellReference.Location));
        if (construct is null) return cellReference with { Dependencies = dependencies.ToList() };

        return construct switch
        {
            Table table => cellReference with { Dependencies = dependencies.ToList() },
            Chain chain => ChainCellReference(chain, cellReference, dependencies),
            _ => cellReference with { Dependencies = dependencies.ToList() },
        };

    }

    private ComputeUnit ChainCellReference(Chain chain, CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        // Check which column it references
        ChainColumn? column = chain.Columns.SingleOrDefault(c => c.Location.Contains(cellReference.Location));

        if (column is null) return cellReference with { Dependencies = dependencies.ToList() };

        // If the location is within the footer, it should be a footer operation
        // TODO: do something with the footer

        if (column is RecursiveChainColumn recursiveColumn)
        {
            // Look if the cell references to the last cell of the column
            if (recursiveColumn.Location.Contains(cellReference.Location))
            {
                int recursionLevel = recursiveColumn.Location.Select(x => x).ToList().IndexOf(cellReference.Location);
            }

        }

    }

    protected override ComputeUnit Other(ComputeUnit unit, IEnumerable<ComputeUnit> dependencies)
    {
        return unit switch
        {
            TableColumn.CellReference c => TableCellReference(c, dependencies),
            ComputedChainColumn.CellReference c => ComputedCellReference(c, dependencies),
            RecursiveChainColumn.RecursiveCellReference c => RecursiveCellReference(c, dependencies),
            _ => throw new InvalidOperationException()
        };
    }

    private ComputeUnit RecursiveCellReference(RecursiveChainColumn.RecursiveCellReference unit, IEnumerable<ComputeUnit> _)
    {

    }

    private ComputeUnit ComputedCellReference(ComputedChainColumn.CellReference unit, IEnumerable<ComputeUnit> _)
    {

    }

    private ComputeUnit TableCellReference(TableColumn.CellReference unit, IEnumerable<ComputeUnit> dependencies)
    {

    }
}
