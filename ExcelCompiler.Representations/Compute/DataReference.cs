using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public record DataReference(Location location) : ComputeUnit(location)
{
    public string RepositoryName { get; set; }
    public string DataName { get; set; }
}