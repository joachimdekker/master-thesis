using System.Diagnostics;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Structure;
using Range = ExcelCompiler.Domain.Compute.Range;

namespace ExcelCompiler.Transformations;

public class LinkDependencies
{
    public SupportGraph Link(List<ComputeUnit> cells, IEnumerable<Location> roots)
    {
        // First link everything
        // Find all References and Ranges in the units and add dependencies.
        Dictionary<Location, ComputeUnit> cellDictionary = cells.ToDictionary(cell => cell.Location);

        foreach (ComputeUnit cell in cells)
        {
            Link(cell, cellDictionary);
        }
        
        // Construct the support graph
        List<ComputeUnit> rootUnits = roots.Select(r => cellDictionary[r]).ToList();
        return new SupportGraph
        {
            Roots = rootUnits,
            Cells = cellDictionary,
        };
    }

    private void Link(ComputeUnit cell, Dictionary<Location, ComputeUnit> cellDictionary)
    {
        switch (cell)
        {
            case Reference reference:
                Debug.Assert(reference.Dependencies.Count == 0);
                var unit = cellDictionary[reference.CellReference];
                reference.AddDependency(unit);
                break;
            case Range range:
                Debug.Assert(range.Dependencies.Count == 0);
                
                // We consider every cell in the range to be a dependency of the range
                // This will probably bite us in the butt in the future, for example with SUMIF logic
                // If I think about it now, we could probably just add the range as a dependency, and then
                // The cells in that range as a dependency of the range. Problem solved.
                foreach (var location in range.GetLocations())
                {
                    var rangeUnit = cellDictionary[location];
                    range.AddDependency(rangeUnit);
                }

                break;
                // Do something with range, we don't do anything with this now.
            default:
                foreach (ComputeUnit dependencies in cell.Dependencies.ToList())
                {
                    Link(dependencies, cellDictionary);
                }
                break;
        }
    }
}