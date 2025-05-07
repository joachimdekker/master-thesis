using System.Diagnostics;
using ExcelCompiler.Domain.Structure;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Structure;
using ComputeTable = ExcelCompiler.Representations.Compute.Specialized.Table;
using Range = ExcelCompiler.Representations.Structure.Range;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;

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

    // public ComputeUnit Transform(ComputeUnit old, Dictionary<Location, ComputeUnit> symbols,
    //     Dictionary<Location, ComputeUnit> units, List<ComputeTable> tables)
    // {
    //     ComputeUnit @new;
    //     switch(old)
    //     {
    //         case CellReference { Reference: { } reference }:
    //             if (!symbols.TryGetValue(reference, out @new!))
    //             {
    //                 var dependency = Transform(units[reference], symbols, units, tables);
    //                 symbols[reference] = @new;
    //                 @new = new CellReference(old.Location, reference);
    //                 
    //                 // Add the dependency
    //                 @new.AddDependency(dependency);
    //             }
    //             break;
    //         
    //         case RangeReference { Reference: { } reference }:
    //             List<ComputeUnit> dependencies = [];
    //             foreach (var location in reference.GetLocations())
    //             {
    //                 var dependency = symbols.TryGetValue(location, out var dep) ? dep : Transform(units[location], symbols, units, tables);
    //                 dependencies.Add(dependency);
    //             }
    //             
    //             @new = new RangeReference(old.Location, reference);
    //             
    //             // Add the dependencies
    //             foreach (var computeUnit in dependencies)
    //             {
    //                 @new.AddDependency(computeUnit);
    //             }
    //
    //             break;
    //         
    //         case TableReference { Reference: { } reference }:
    //             var table = tables.Single(t => t.Name == reference.TableName);
    //             
    //             @new = new TableReference(old.Location, reference);
    //
    //             break;
    //         
    //         
    //         
    //         default: throw new InvalidOperationException("Unsupported cell type.");
    //     };
    //     
    //     return @new;
    // }
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