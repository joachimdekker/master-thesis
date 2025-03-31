using System.Text.Json;
using ExcelCompiler.Domain.Spreadsheet;

namespace ExcelCompiler.Domain.Compute;

public class FunctionComposition : ComputeUnit
{
    public string Name { get; internal set; }
    
    public List<ComputeUnit> Arguments { get; internal set; }

    public FunctionComposition(string name, List<ComputeUnit> arguments, Location location)
    : base(location)
    {
        Name = name;
        Arguments = arguments;
    }
    
    public FunctionComposition(string name, List<ComputeUnit> arguments, Location location, string raw)
        : base(location)
    {
        Name = name;
        Arguments = arguments;
        Raw = raw;
    }
    
    public override string ToString() => JsonSerializer.Serialize(this);
}