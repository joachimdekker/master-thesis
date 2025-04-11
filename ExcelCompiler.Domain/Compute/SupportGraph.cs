using ExcelCompiler.Domain.Spreadsheet;

namespace ExcelCompiler.Domain.Compute;

/// <summary>
/// Represents a structure that contains a graph of functions and their dependencies.
/// </summary>
/// <remarks>
/// We use this structure for efficient evaluation of the dependencies.
/// This class is used to calculate dependencies and interesting structures.
/// </remarks>
public class SupportGraph
{
    /// <summary>
    /// Gets or sets the roots of the support graph.
    /// </summary>
    /// <remarks>
    /// The roots are the nodes that store the results of the calculations.
    /// There can be multiple roots, since we support multiple outcomes in the program.
    /// </remarks>
    public List<ComputeUnit> Roots { get; init; } = [];
    
    public Dictionary<Location,ComputeUnit> Cells { get; init; } = [];
    
    public SupportGraph() { }
    
    public SupportGraph(List<ComputeUnit> roots)
    {
        Roots = roots;
    }

    /// <summary>
    /// Gets the topological sorted order of the cells in the support graph.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ComputeUnit> TopologicalSortedByCell()
    {
        HashSet<Location> visited = new HashSet<Location>();

        foreach (var cell in Roots.SelectMany(root => TopologicalSortedByCell(root, visited)))
        {
            yield return cell;
        }
    }
    
    private static IEnumerable<ComputeUnit> TopologicalSortedByCell(ComputeUnit cell, HashSet<Location> visited)
    {
        if (visited.Add(cell.Location))
        {
            yield return cell;
        }

        foreach (var dep in cell.Dependencies.SelectMany(dependency => TopologicalSortedByCell(dependency, visited)))
        {
            yield return dep;
        }
    }
}