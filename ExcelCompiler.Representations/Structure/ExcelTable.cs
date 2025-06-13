using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

public class ExcelTable
{
    public string Name { get; set; }
    public Range Location { get; set; }
}