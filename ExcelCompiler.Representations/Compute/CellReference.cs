using ExcelCompiler.Representations.Structure;

namespace ExcelCompiler.Representations.Compute;

public class CellReference : ComputeUnit
{
    public CellReference(Location location, Location reference) : base(location)
    {
        Reference = reference;
    }

    public Location Reference { get; set; }
    public override bool IsConstant => false;
}