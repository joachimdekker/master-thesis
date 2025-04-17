using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Location = ExcelCompiler.Domain.Structure.Location;

namespace ExcelCompiler.Domain.Compute;

/// <summary>
/// Represents a unit of computation in the spreadsheet.
/// </summary>
/// <remarks>
/// This class is used to represent a function and its dependencies.
/// It can be a single function or a group of functions. As small or big as one desires.
/// It is used as an abstraction in multiple stages in the compilation.
/// </remarks>
public abstract class ComputeUnit
{
    public List<ComputeUnit> Dependencies { get; init; }
    public List<ComputeUnit> Dependents { get; init; }
    public Location Location { get; init; }
    
    public bool IsRoot => Dependents.Count == 0;
    
    public bool IsLeaf => Dependencies.Count == 0;
    
    public string? Raw { get; internal set; }

    public abstract bool IsConstant { get; }
    
    public ComputeUnit(Location location)
    {
        Location = location;
        Dependencies = [];
        Dependents = [];
    }
    
    public void AddDependency(ComputeUnit dependency)
    {
        Dependencies.Add(dependency);
        dependency.Dependents.Add(this);
    }
    
    public void RemoveDependency(ComputeUnit dependency)
    {
        Dependencies.Remove(dependency);
        dependency.Dependents.Remove(this);
    }
    
    public override string ToString() => JsonSerializer.Serialize(this);

    public bool TryGetByLocation(Location location, [NotNullWhen(true)] out ComputeUnit? computeUnit)
    {
        computeUnit = null!;
        if (location == Location)
        {
            computeUnit = this;
            return true;
        }

        foreach (var t in Dependencies)
        {
            var found = t.TryGetByLocation(location, out computeUnit);
            if (found)
            {
                return found;
            }
        }

        return false;
    }
}