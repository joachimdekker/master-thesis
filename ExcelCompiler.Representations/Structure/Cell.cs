namespace ExcelCompiler.Representations.Structure;

public abstract record Cell
{
    protected Cell(Location location, Type type)
    {
        Location = location;
        Type = type;
    }

    public Location Location { get; init; }
    
    public Type Type { get; init; }
}

public record EmptyCell : Cell
{
    public EmptyCell(Location location) : base(location, null!)
    {
    }
}