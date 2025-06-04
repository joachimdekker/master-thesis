using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

public class Chain : Construct
{
    public Dictionary<string, Range> Columns { get; init; } = [];
}