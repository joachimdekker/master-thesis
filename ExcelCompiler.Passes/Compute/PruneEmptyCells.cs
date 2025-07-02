using ExcelCompiler.Representations.Compute;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class PruneEmptyCells
{
    private readonly HashSet<ComputeUnit> _visited = new();
    
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
        IEnumerable<ComputeUnit> dependencies = node.Dependencies.Where(d => d is not Nil).Select(Prune);
        
        return node with
        {
            Dependencies = dependencies.ToList(),
        };
    }
}
