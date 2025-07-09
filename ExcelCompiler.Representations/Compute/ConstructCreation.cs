using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public record ConstructCreation : ComputeUnit
{
    public ConstructCreation(Location location, string constructId) : base(location)
    {
        ConstructId = constructId;
    }

    public string ConstructId { get; set; }
}