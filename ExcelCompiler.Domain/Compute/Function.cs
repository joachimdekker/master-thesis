using ExcelCompiler.Domain.Spreadsheet;

namespace ExcelCompiler.Domain.Compute;

public class Function : ComputeUnit
{
    public Function(string name, Location location) : base(location)
    {
        Name = name;
    }

    public string Name { get; }
}