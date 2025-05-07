using ExcelCompiler.Representations.Structure;

namespace ExcelCompiler.Representations.Compute;

public class Nil(Location location) : ComputeUnit(location)
{
    public override bool IsConstant => true;
}