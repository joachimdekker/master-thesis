using ExcelCompiler.Domain.Structure;
using Range = ExcelCompiler.Domain.Structure.Range;

namespace ExcelCompiler.Domain.Compute;

public class RangeReference : ComputeUnit
{
    public RangeReference(Location location, Range reference) : base(location)
    {
        Reference = reference;
    }

    public Range Reference { get; set; }
    
    public override bool IsConstant => false;
}