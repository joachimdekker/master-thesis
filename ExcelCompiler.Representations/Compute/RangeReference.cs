using ExcelCompiler.Representations.Structure;
using Location = ExcelCompiler.Representations.References.Location;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Compute;

public record RangeReference : ComputeUnit
{
    public RangeReference(Location location, Range reference) : base(location)
    {
        Reference = reference;
    }

    public Range Reference { get; set; }
}