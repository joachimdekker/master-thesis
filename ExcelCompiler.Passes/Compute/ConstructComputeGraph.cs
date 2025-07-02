using System.Diagnostics;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Chain = ExcelCompiler.Representations.Compute.Specialized.Chain;
using ComputeTable = ExcelCompiler.Representations.Compute.Specialized.Table;
using Construct = ExcelCompiler.Representations.Compute.Specialized.Construct;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class ConstructComputeGraph
{
    public ComputeGraph Transform(ComputeGrid units, List<Location> outputs)
    {
        LinkTransformer transformer = new(units);

        // Create the support graph from the units
        // Let's try a new approach and use immutable data structures
        List<ComputeUnit> roots = outputs
            .Select(outputLocation => units[outputLocation])
            .Select(unlinkedUnit => transformer.Transform(unlinkedUnit))
            .ToList();

        // Transform tables
        foreach (var construct in units.Structures)
        {
            if (construct is ComputeTable table)
            {
                foreach (var column in table.Columns.Where(c => c.ColumnType is TableColumn.TableColumnType.Computed))
                {
                    var computation = column.Computation;
                    var unit = transformer.Transform(computation!);
                    column.Computation = unit;
                }
            } else if (construct is Chain chain)
            {
                foreach (var column in chain.Columns.OfType<ComputedChainColumn>())
                {
                    var computation = column.Computation;
                    var unit = transformer.Transform(computation!);
                    column.Computation = unit;
                }

                foreach (var column in chain.Columns.OfType<RecursiveChainColumn>())
                {
                    var computation = column.Computation;
                    var unit = transformer.Transform(computation!);
                    column.Computation = unit;
                }
            }
            
        }

        return new ComputeGraph
        {
            Roots = roots,
            Constructs = units.Structures,
        };
    }
    
    
}


public record LinkTransformer : UnitComputeGraphTransformer
{
    private readonly ComputeGrid _units;
    private Dictionary<ComputeUnit, ComputeUnit> _cache = new();

    public LinkTransformer(ComputeGrid units)
    {
        _units = units;
    }

    public override ComputeGraph Transform(ComputeGraph graph)
    {
        _cache = [];

        var supportGraph = base.Transform(graph);

        return supportGraph;
    }

    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        Debug.WriteLineIf(!dependencies.Any(), "Cell should not have any dependencies yet.");

        // Lookup the reference and transform it
        var unit = _units[cellReference.Reference];
        var linkedUnit = Transform(unit);

        return cellReference with
        {
            Dependencies = [linkedUnit],
        };
    }

    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        Debug.WriteLineIf(!dependencies.Any(), "Range should not have any dependencies yet.");

        // Lookup the reference and transform it
        var deps = rangeReference.Reference
            .Select(l => _units[l])
            .Select(Transform)
            .ToList();

        return rangeReference with
        {
            Dependencies = deps,
        };
    }
}
