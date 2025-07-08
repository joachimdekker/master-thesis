using ExcelCompiler.Representations.Compute.Specialized;
using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

/// <summary>
/// Represents a structure that contains a graph of functions and their dependencies.
/// </summary>
/// <remarks>
/// We use this structure for efficient evaluation of the dependencies.
/// This class is used to calculate dependencies and interesting structures.
/// </remarks>
public record ComputeGraph
{
    /// <summary>
    /// Gets or sets the roots of the support graph.
    /// </summary>
    /// <remarks>
    /// The roots are the nodes that store the results of the calculations.
    /// There can be multiple roots, since we support multiple outcomes in the program.
    /// </remarks>
    public List<ComputeUnit> Roots { get; init; } = [];

    public IEnumerable<Input> Inputs => Roots.SelectMany(r => r.GetByType<Input>());

    public List<Construct> Constructs { get; init; } = [];

    public ComputeGraph() { }

    public ComputeGraph(List<ComputeUnit> roots)
    {
        Roots = roots;
    }

    public IEnumerable<ComputeUnit> TopologicalSorted()
    {
        HashSet<ComputeUnit> visited = new HashSet<ComputeUnit>();

        foreach (var cell in Roots.SelectMany(root => TopologicalSorted(root, visited)))
        {
            yield return cell;
        }
    }

    private static IEnumerable<ComputeUnit> TopologicalSorted(ComputeUnit cell, HashSet<ComputeUnit> visited)
    {
        if (!visited.Add(cell))
        {
            yield break;
        }

        foreach (var dep in cell.Dependencies.SelectMany(dependency => TopologicalSorted(dependency, visited)))
        {
            yield return dep;
        }

        yield return cell;
    }

    /// <summary>
    /// Get the entry points of all cells, topologically sorted.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ComputeUnit> EntryPointsOfCells()
    {
        HashSet<Location> visited = new HashSet<Location>();
        
        IEnumerable<ComputeUnit> topologicalSorted = TopologicalSorted();
        
        // Reverse the order, since we want to start with the leaves.
        foreach (var computeUnit in topologicalSorted.Reverse())
        {
            if (!visited.Add(computeUnit.Location))
            {
                continue;
            }

            yield return computeUnit;
        }
    }
}
