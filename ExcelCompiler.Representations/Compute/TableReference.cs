using Location = ExcelCompiler.Representations.Structure.Location;

namespace ExcelCompiler.Representations.Compute;

public class TableReference : ComputeUnit
{
    public TableReference(Location location, Structure.TableReference reference) : base(location)
    {
        Reference = reference;
    }

    public Structure.TableReference Reference { get; set; }
}