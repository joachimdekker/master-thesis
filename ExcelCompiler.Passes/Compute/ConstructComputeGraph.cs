using System.Diagnostics;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Structure;
using ComputeTable = ExcelCompiler.Representations.Compute.Specialized.Table;
using Range = ExcelCompiler.Representations.Structure.Range;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class ConstructComputeGraph
{
    public SupportGraph Transform(Dictionary<Location, ComputeUnit> units, List<ComputeTable> tables, List<Location> outputs)
    {
        LinkTransformer transformer = new(units, tables);
        
        // Create the support graph from the units
        // Let's try a new approach and use immutable data structures
        List<ComputeUnit> roots = [];
        
        foreach (var outputLocation in outputs)
        {
            var unlinkedUnit = units[outputLocation];

            var linkedUnit = transformer.Transform(unlinkedUnit);
            
            roots.Add(linkedUnit);
        }

        return new SupportGraph
        {
            Roots = roots,
            Tables = tables,
        };
    }
}


public record LinkTransformer : UnitSupportGraphTransformer
{
    private readonly Dictionary<Location, ComputeUnit> _units;
    private readonly List<ComputeTable> _tables;
    private Dictionary<ComputeUnit, ComputeUnit> _cache = new();

    public LinkTransformer(Dictionary<Location, ComputeUnit> units, List<ComputeTable> tables)
    {
        _units = units;
        _tables = tables;
    }

    public override SupportGraph Transform(SupportGraph graph)
    {
        _cache = [];
        return base.Transform(graph);
    }

    public override ComputeUnit Transform(ComputeUnit unit)
    {
        return Transform(unit, _cache);
    }

    protected override ComputeUnit CellReference(Location location, IEnumerable<ComputeUnit> dependencies,
        Location reference)
    {
        Debug.WriteLineIf(!dependencies.Any(), "Cell should not have any dependencies yet.");
        
        var cellReference = new CellReference(location, reference);
        
        // Lookup the reference and transform it
        var unit = _units[reference];
        var linkedUnit = Transform(unit);
        
        // Add the dependency
        cellReference.AddDependency(linkedUnit);
        
        return cellReference;
    }

    protected override ComputeUnit RangeReference(Location location, IEnumerable<ComputeUnit> dependencies, Range reference)
    {
        Debug.WriteLineIf(!dependencies.Any(), "Range should not have any dependencies yet.");
        
        var rangeReference = new RangeReference(location, reference);
        
        // Lookup the reference and transform it
        var deps = reference.GetLocations()
            .Select(l => _units[l])
            .Select(Transform);
        rangeReference.AddDependencies(deps);
        
        
        return rangeReference;
    }
}