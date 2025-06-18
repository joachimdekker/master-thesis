using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public record Function : ComputeUnit
{
    public Function(Location location, string name) : base(location)
    {
        Name = name;
    }

    public Function(Location location, string name, params IEnumerable<ComputeUnit> parameters) : this(location, name)
    {
        AddDependencies(parameters);
    }
    
    public string Name { get; }
}