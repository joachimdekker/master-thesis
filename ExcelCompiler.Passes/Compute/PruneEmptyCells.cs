using ExcelCompiler.Representations.Compute;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class PruneEmptyCells
{
    public SupportGraph Transform(SupportGraph graph)
    {
        HashSet<ComputeUnit> visited = new();
        
        // Traverse the support graph and remove empty cells
        foreach (var root in graph.Roots.ToList())
        {
            // If the root is an empty cell, remove it
            if (root is Nil)
            {
                PruneNode(root);
                graph.Roots.Remove(root);
                continue;
            }
            
            TraverseAndPrune(root, visited);
        }
        
        return graph;
    }

    private void TraverseAndPrune(ComputeUnit node, HashSet<ComputeUnit> visited)
    {
        if (!visited.Add(node))
        {
            return;
        }
        
        if (node is Nil)
        {
            PruneNode(node);
            return;
        }

        foreach (var dependency in node.Dependencies.ToList())
        {
            TraverseAndPrune(dependency, visited);
        }
    }

    private void PruneNode(ComputeUnit node)
    {
        foreach (var dependent in node.Dependents.ToList())
        {
            dependent.RemoveDependency(node);
        }

        if (node.Dependencies.Count != 0)
        {
            throw new InvalidOperationException("Something is horribly wrong. We have pruned an empty node with known dependencies...");
        }
    }
}