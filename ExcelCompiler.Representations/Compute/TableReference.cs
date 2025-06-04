using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public class TableReference : ComputeUnit
{
    public TableReference(Location location, References.TableReference reference) : base(location)
    {
        Reference = reference;
    }

    public References.TableReference Reference { get; set; }
}