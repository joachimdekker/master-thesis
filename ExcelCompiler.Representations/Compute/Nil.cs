using ExcelCompiler.Domain.Structure;

namespace ExcelCompiler.Domain.Compute;

public class Nil(Location location) : ComputeUnit(location)
{
    public override bool IsConstant => true;
}