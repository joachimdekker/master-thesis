using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public record Input : ComputeUnit
{
    public Input(Type type, Location location) : base(location)
    {
        Type = type;
    }
}