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
    
    public SupportGraph() { } 
    
    public SupportGraph(List<ComputeUnit> roots)
    {
        Roots = roots;
    }

    /// <summary>
    /// Gets the entry point of certain location in the support graph.
    /// </summary>
    /// <remarks>
    /// This method is used primarily for resolving references. We pick the first point from the root where the location matches.
    /// </remarks>
    /// <param name="referenceCellReference"></param>
    /// <returns></returns>
    public ComputeUnit GetReference(Location referenceCellReference)
    {
        IEnumerable<ComputeUnit> Traverse(ComputeUnit unit)
        {
            yield return unit;
            foreach (var child in unit.Dependencies.SelectMany(Traverse))
            {
                yield return child;
            }
        }
        
        foreach (var root in Roots)
        {
            var unit = Traverse(root).FirstOrDefault(x => x.Location == referenceCellReference);
            if (unit is not null)
            {
                return unit;
            }
        }
        
        throw new InvalidOperationException($"No entry point found for {referenceCellReference}");
    }
}