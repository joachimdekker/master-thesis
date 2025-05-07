using Location = ExcelCompiler.Representations.Structure.Location;

namespace ExcelCompiler.Representations.Compute;

public class Function : ComputeUnit
{
    public Function(Location location, string name) : base(location)
    {
        Name = name;
    }
    
    public string Name { get; }  // "-"
    public override bool IsConstant => false;
}