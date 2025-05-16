using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.Structure.Range;

namespace ExcelCompiler.Representations.Compute;

public class RangeReference : ComputeUnit
{
    public RangeReference(Location location, Range reference) : base(location)
    {
        Reference = reference;
    }

    public Range Reference { get; set; }
}