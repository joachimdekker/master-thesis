using Location = ExcelCompiler.Domain.Structure.Location;

namespace ExcelCompiler.Domain.Compute;

public class TableReference : ComputeUnit
{
    public TableReference(Location location, Structure.TableReference reference) : base(location)
    {
        Reference = reference;
    }

    public Structure.TableReference Reference { get; set; }
    
    public override bool IsConstant => false;
}