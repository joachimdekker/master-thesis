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
    public Type Type { get; set; }
    public List<ComputeUnit> Dependencies { get; init; }
    public List<ComputeUnit> Dependents { get; init; }
    public Location Location { get; init; }

    public bool IsRoot => Dependents.Count == 0;

    public bool IsLeaf => Dependencies.Count == 0;

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

    public void AddDependencies(IEnumerable<ComputeUnit> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            AddDependency(dependency);
        }
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

    public IEnumerable<T> GetByType<T>()
    {
        IEnumerable<T> list = Dependencies.SelectMany(dependency => dependency.GetByType<T>());

        if (this is T t)
        {
            return list.Concat([t]);
        }

        return list;
    }

    public int CountByType<T>()
    where T : ComputeUnit
    {
        return (this is T) ? 1 : 0 + Dependencies.Sum(dependency => dependency.CountByType<T>());
    }

    public bool HasType<T>()
    where T : ComputeUnit
    {
        return (this is T) || Dependencies.Any(dependency => dependency.HasType<T>());
    }
}
