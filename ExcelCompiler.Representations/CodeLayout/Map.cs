namespace ExcelCompiler.Representations.CodeLayout;

public record Map : Type
{
    public Type Domain { get; init; }
    
    public Type Range { get; init; }
    
    public Map(Type domain, Type range) : base($"Dictionary<{domain.Name}, {range.Name}>")
    {
        Domain = domain;
        Range = range;
    }
}