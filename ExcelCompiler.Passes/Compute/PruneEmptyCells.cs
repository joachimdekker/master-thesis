using ExcelCompiler.Representations.Compute;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class PruneEmptyCells
{
    private readonly Dictionary<ComputeUnit, ComputeUnit> _visited = new();
    
    public ComputeGraph Transform(ComputeGraph graph)
    {

        List<ComputeUnit> roots = graph.Roots.Where(r => r is not Nil).Select(cu => Prune(cu)).ToList();

        return graph with
        {
            Roots = roots,
        };
    }

    private ComputeUnit Prune(ComputeUnit node)
    {
        if (_visited.TryGetValue(node, out var unit)) return unit;
        
        IEnumerable<ComputeUnit> dependencies = PruneDependencies(node);
        
        _visited[node] = node with
        {
            Dependencies = dependencies.ToList(),
        };

        return _visited[node];
    }

    private IEnumerable<ComputeUnit> PruneDependencies(ComputeUnit node)
    {
        foreach (var dependency in node.Dependencies)
        {
            if (dependency is not Nil)
            {
                yield return Prune(dependency);
                continue;
            }
            
            // Check for default values
            if (dependency.Type?.Name is "double" or "Double")
            {
                yield return new ConstantValue<double>(0, dependency.Location);
            }
        }
    }
}
