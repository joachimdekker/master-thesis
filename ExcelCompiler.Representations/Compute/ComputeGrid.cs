using System.Collections;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.References;

namespace ExcelCompiler.Representations.Compute;

/// <summary>
/// Represents the computations that are performed within the workbook in the spreadsheets.
/// </summary>
public record ComputeGrid : IEnumerable<ComputeUnit>
{
    private Dictionary<Location, ComputeUnit> _units = new();

    public ComputeUnit this[Location location]
    {
        get => _units[location];
        set => _units[location] = value;
    }

    public List<Construct> Structures { get; set; } = new();

    public bool ContainsLocation(Location location) => _units.ContainsKey(location);

    public bool TryGetValue(Location location, out ComputeUnit unit) => _units.TryGetValue(location, out unit);

    public IEnumerator<ComputeUnit> GetEnumerator()
    {
        return _units.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
