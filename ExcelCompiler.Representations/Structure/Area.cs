namespace ExcelCompiler.Representations.Structure;

public record Area
{
    public Range Range { get; init; }
    
    public Area(Range range)
    {
        Range = range;
    }
}