using ExcelCompiler.Representations.Structure;
using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public class CellReference : ComputeUnit
{
    public CellReference(Location location, Location reference) : base(location)
    {
        Reference = reference;
    }

    public Location Reference { get; set; }
}