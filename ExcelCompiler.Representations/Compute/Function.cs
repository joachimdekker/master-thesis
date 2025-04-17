using Location = ExcelCompiler.Domain.Structure.Location;

namespace ExcelCompiler.Domain.Compute;

public class Function : ComputeUnit
{
    public Function(string name, Location location) : base(location)
    {
        Name = name;
    }
    
    

    public string Name { get; }
    public override bool IsConstant => false;
}