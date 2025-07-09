using ExcelCompiler.Representations.References;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Compute.Specialized;

public record Construct(string Id, Range Location)
{
    public bool IsInput { get; set; } = false;
}