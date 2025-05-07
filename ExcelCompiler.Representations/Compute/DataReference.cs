using Location = ExcelCompiler.Representations.Structure.Location;

namespace ExcelCompiler.Representations.Compute;

public class DataReference(Location location) : ComputeUnit(location)
{
    public string RepositoryName { get; set; }
    public string DataName { get; set; }
    
    public override bool IsConstant => true;
}