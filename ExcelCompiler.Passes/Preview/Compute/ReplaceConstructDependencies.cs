using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class ReplaceConstructDependencies
{
    public ComputeGraph Transform(ComputeGraph graph)
    {
        var transformer = new RecursiveTypeTransformer(graph.Constructs);
        return transformer.Transform(graph);
    }
}

file record RecursiveTypeTransformer(List<Construct> Constructs) : UnitComputeGraphTransformer()
{
    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        // Check if the range references a table column
        var construct = Constructs.SingleOrDefault(c => c.Location.Contains(rangeReference.Reference));
        if (construct is null) return rangeReference with { Dependencies = dependencies.ToList() };

        // Get the type of the range based on the type of the construct
        return construct switch
        {
            Table table when table.Columns.Any(c => c.Location.Contains(rangeReference.Reference))
                => new ColumnOperation(table, table.Columns.Single(c => c.Location.Contains(rangeReference.Reference)).Name, rangeReference.Location),
            Chain chain => ChainRangeReference(chain, rangeReference, dependencies),
            _ => rangeReference with {Dependencies = dependencies.ToList()},
        };
    }

    private ComputeUnit ChainRangeReference(Chain chain, RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        ChainColumn? column = chain.Columns.SingleOrDefault(c => c.Location.Contains(rangeReference.Reference));

        if (column is null) return rangeReference with { Dependencies = dependencies.ToList() };

        // TODO: Perhaps something to easily detect something
        return new ColumnOperation(chain, column.Name, rangeReference.Location);
    }

    protected override ComputeUnit TableReference(TableReference tableReference, IEnumerable<ComputeUnit> dependencies)
    {
        return tableReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit DataReference(DataReference dataReference, IEnumerable<ComputeUnit> dependencies)
    {
        if (!Constructs.Any(c => c.Location.Contains(dataReference.Location))) return dataReference with { Dependencies = dependencies.ToList() };
        
        Construct construct = Constructs.Single(c => c.Location.Contains(dataReference.Location));
        CellReference cellRef = new CellReference(dataReference.Location, dataReference.Location) { Type = dataReference.Type, Dependencies = dependencies.ToList() };

        return Transform(cellRef);
    }

    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        var construct = Constructs.SingleOrDefault(c => c.Location.Contains(cellReference.Reference));
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
        ChainColumn? column = chain.Columns.SingleOrDefault(c => c.Location.Contains(cellReference.Reference));

        if (column is null) return cellReference with { Dependencies = dependencies.ToList() };

        // If the location is within the footer, it should be a footer operation
        // TODO: do something with the footer

        if (column is RecursiveChainColumn recursiveColumn)
        {
            // Look if the cell references to the last cell of the column
            if (recursiveColumn.Location.Contains(cellReference.Location))
            {
                int recursionLevel = recursiveColumn.Location.Select(x => x).ToList().IndexOf(cellReference.Reference);
                return new RecursiveResultReference(recursiveColumn.Name, chain.Name, recursionLevel, cellReference.Location);
            }
        }
        
        return cellReference with { Dependencies = dependencies.ToList() };
    }
}
