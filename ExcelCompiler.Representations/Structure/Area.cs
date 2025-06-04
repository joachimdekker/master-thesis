using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

public record Area
{
    public Range Range { get; init; }

    public string? Spreadsheet => Range.Spreadsheet;
    
    public Area(Range range)
    {
        Range = range;
    }
}