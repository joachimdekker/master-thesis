using System.Collections;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.References;

namespace ExcelCompiler.Representations.Compute;

/// <summary>
/// Represents the computations that are performed within the workbook in the spreadsheets.
/// </summary>
public class ComputeGrid : IEnumerable<ComputeUnit>
{
    private Dictionary<Location, ComputeUnit> _units = new();

    public ComputeUnit this[Location location]
    {
        get => _units[location];
        set => _units[location] = value;
    }

    public List<Construct> Structures { get; set; } = new();

    public bool ContainsLocation(Location location) => _units.ContainsKey(location);
    
    public IEnumerator<ComputeUnit> GetEnumerator()
    {
        return _units.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}