using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

/// <summary>
/// Represents a unit of computation in the spreadsheet.
/// </summary>
/// <remarks>
/// This class is used to represent a function and its dependencies.
/// It can be a single function or a group of functions. As small or big as one desires.
/// It is used as an abstraction in multiple stages in the compilation.
/// </remarks>
public abstract record ComputeUnit
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string? Note { get; set; }
    public Type? Type { get; set; }
    public IReadOnlyList<ComputeUnit> Dependencies { get; init; }
    public Location Location { get; init; }

    public bool IsLeaf => Dependencies.Count == 0;

    public ComputeUnit(Location location)
    {
        Location = location;
        Dependencies = [];
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

    // public override int GetHashCode()
    // {
    //     Dependencies.Aggregate((curr, next) => {HashCode.Combine()})
    //     return HashCode.Combine(Location, );
    // }

    public bool ComputationalEquivalent(ComputeUnit other)
    {
        if (other.GetType() != GetType()) return false;
        if (other.Dependencies.Count != Dependencies.Count) return false;

        return !Dependencies.Where((t, i) => !t.ComputationalEquivalent(other.Dependencies[i])).Any();
    }

    public List<T> GetByType<T>()
    {
        var cache = new Dictionary<ComputeUnit, List<T>>();

        return Traverse(this);
        
        List<T> Traverse(ComputeUnit unit)
        {
            if (cache.TryGetValue(unit, out var result))
            {
                return result;
            }
            
            List<T> list = unit.Dependencies.Where(d => !cache.ContainsKey(d)).SelectMany(Traverse).ToList();
            if (unit is T t)
            {
                return cache[unit] = [t, ..list];
            }

            return cache[unit] = list;
        }
        
    }

    public bool HasType<T>()
    where T : ComputeUnit
    {
        return (this is T) || Dependencies.Any(dependency => dependency.HasType<T>());
    }

    public virtual bool Equals(ComputeUnit? other)
    {
        return other is not null 
               && other.Location == Location 
               && other.Dependencies.Count == Dependencies.Count 
               && Dependencies.SequenceEqual(other.Dependencies);
    }

    private int _hashCode = -1;

    public override int GetHashCode()
    {
        if (_hashCode != -1) return _hashCode;
        
        const int mult = 31;
        const int seed = 0x2D2816FE;
        var res = Dependencies.Aggregate(seed, (current, dependency) => current * mult + dependency?.GetHashCode() ?? 0);
        res = res * mult + GetType().GetHashCode();
        return _hashCode = res * mult + Location.GetHashCode();
    }
}
