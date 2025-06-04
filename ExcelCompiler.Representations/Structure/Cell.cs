namespace ExcelCompiler.Representations.Structure;

public abstract record Cell
{
    protected Cell(References.Location location, Type type)
    {
        Location = location;
        Type = type;
    }

    public References.Location Location { get; init; }
    
    public Type Type { get; init; }
}

public record EmptyCell : Cell
{
    public EmptyCell(References.Location location) : base(location, null!)
    {
    }
}