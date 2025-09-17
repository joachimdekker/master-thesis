using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using Type = ExcelCompiler.Representations.Compute.Type;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class ReplaceConstructDependencies
{
    public ComputeGraph Transform(ComputeGraph graph)
    {
        var constructGenerators = graph.Constructs.Select(Generate).ToList();
        var transformer = new RecursiveTypeTransformer(graph.Constructs, constructGenerators);
        return transformer.Transform(graph);
    }

    private ConstructCreation Generate(Construct arg)
    {
        List<ComputeUnit> dependencies = arg switch
        {
            { IsInput: true } => [],
            Table table => table.StructureData!.Columns.SelectMany(c => c.Data)
                .Concat(table.StructureData.Constants.Select(c => c.Value))
                .ToList(),
            Chain chain => chain.StructureData!.Columns.SelectMany(c => c.Data)
                .Concat(chain.StructureData.Initialisations.SelectMany(c => c.Data))
                .Concat(chain.StructureData.Constants.Select(c => c.Value))
                .ToList(),
            _ => new List<ComputeUnit>(),
        };
        
        return new ConstructCreation(arg.Location.From, arg.Id)
        {
            Dependencies = dependencies,
            Type = new Type(arg.Id),
        };
    }
}

file record RecursiveTypeTransformer(List<Construct> Constructs, List<ConstructCreation> Constructors) : UnitComputeGraphTransformer()
{
    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        // Check if the range references a table column
        var construct = Constructs.SingleOrDefault(c => c.Location.Contains(rangeReference.Reference));
        if (construct is null) return rangeReference with { Dependencies = dependencies.ToList() };

        var constructor = Constructors.Single(c => c.ConstructId == construct.Id);

        // Get the type of the range based on the type of the construct
        switch (construct)
        {
            case Table table when table.Columns.Any(c => c.Location.Contains(rangeReference.Reference)):
                return new ColumnOperation(table,
                    table.Columns.Single(c => c.Location.Contains(rangeReference.Reference)).Name,
                    rangeReference.Location)
                {
                    Dependencies = [constructor]
                };
            case Chain chain:
                var reference = ChainRangeReference(chain, rangeReference, dependencies);
                return reference with
                {
                    Dependencies = [constructor]
                };
            default: return rangeReference with { Dependencies = dependencies.ToList() };
        };
    }

    private ComputeUnit ChainRangeReference(Chain chain, RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        ChainColumn? column = chain.Columns.SingleOrDefault(c => c.Location.Contains(rangeReference.Reference));
        
        if (column is null) return rangeReference with { Dependencies = dependencies.ToList() };

        // TODO: Perhaps something to easily detect something
        return new ColumnOperation(chain, column.Name, rangeReference.Location)
        {
            
        };
    }

    protected override ComputeUnit TableReference(TableReference tableReference, IEnumerable<ComputeUnit> dependencies)
    {
        var constructor = Constructors.SingleOrDefault(c => c.ConstructId ==  tableReference.Reference.TableName);

        if (constructor is null) throw new InvalidOperationException("Constructor for table not found.");
        
        return tableReference with { Dependencies = [..dependencies, constructor] };
    }

    protected override ComputeUnit DataReference(DataReference dataReference, IEnumerable<ComputeUnit> dependencies)
    {
        if (!Constructs.Any(c => c.Location.Contains(dataReference.Location))) return dataReference with { Dependencies = dependencies.ToList() };
        
        Construct construct = Constructs.Single(c => c.Location.Contains(dataReference.Location));
        ConstructCreation creation = Constructors.Single(c => c.ConstructId == construct.Id);
        CellReference cellRef = new CellReference(dataReference.Location, dataReference.Location)
        {
            Type = dataReference.Type, 
            Dependencies = [creation, ..dependencies]
        };

        return Transform(cellRef);
    }

    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        var construct = Constructs.SingleOrDefault(c => c.Location.Contains(cellReference.Reference));
        if (construct is null) return cellReference with { Dependencies = dependencies.ToList() };

        var creation = Constructors.Single(c => c.ConstructId == construct.Id);
        
        return construct switch
        {
            Table table => cellReference with { Dependencies = [creation, ..dependencies] },
            Chain chain => ChainCellReference(chain, cellReference, [creation, ..dependencies]),
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
                return new RecursiveResultReference(recursiveColumn.Name, chain.Name, recursionLevel, cellReference.Location)
                {
                    Dependencies = dependencies.ToList(),
                };
            }
        }
        
        return cellReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit Other(ComputeUnit unit, IEnumerable<ComputeUnit> dependencies)
    {
        if (unit is RecursiveChainColumn.RecursiveCellReference or DataChainColumn.Reference or Table.CellReference or TableColumn.CellReference)
        {
            var creation = Constructors.Single(c => c.ConstructId == unit switch
            {
                RecursiveChainColumn.RecursiveCellReference r => r.ChainName,
                DataChainColumn.Reference r => r.ChainName,
                Table.CellReference r => r.TableName,
                TableColumn.CellReference r => r.TableName,
                _ => throw new InvalidOperationException(),
            });
            
            return unit with { Dependencies = [creation, ..dependencies] };
        }
        
        return unit with { Dependencies = dependencies.ToList() };
    }
}
