using ExcelCompiler.Representations.Compute;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class PruneEmptyCells
{
    private readonly Dictionary<ComputeUnit, ComputeUnit> _visited = new();
    
    public ComputeGraph Transform(ComputeGraph graph)
    {

        List<ComputeUnit> roots = graph.Roots.Where(r => r is not Nil).Select(Prune).ToList();

        return graph with
        {
            Roots = roots,
        };
    }

    private ComputeUnit Prune(ComputeUnit node)
    {
        if (_visited.TryGetValue(node, out var unit)) return unit;
        
        IEnumerable<ComputeUnit> dependencies = node.Dependencies.Where(d => d is not Nil).Select(Prune);
        
        _visited[node] = node with
        {
            Dependencies = dependencies.ToList(),
        };

        return _visited[node];
    }
}
