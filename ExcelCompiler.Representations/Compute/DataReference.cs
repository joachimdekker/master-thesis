using Location = ExcelCompiler.Domain.Structure.Location;

namespace ExcelCompiler.Domain.Compute;

public class DataReference(Location location) : ComputeUnit(location)
{
    public string RepositoryName { get; set; }
    public string DataName { get; set; }
    
    public override bool IsConstant => true;
}