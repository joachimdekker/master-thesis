using ExcelCompiler.Representations.Compute;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class LinkIdenticalnodes
{
    public ComputeGraph Transform(ComputeGraph graph)
    {
        var sorted = graph.TopologicalSorted();

        HashSet<ComputeUnit> seen = new();
        
        foreach (var node in sorted)
        {
            if (!seen.Add(node))
            {
                throw new Exception();
            }
        }
        
        return graph;
    }
}