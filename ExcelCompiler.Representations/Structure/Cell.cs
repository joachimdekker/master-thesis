namespace ExcelCompiler.Representations.Structure;

public abstract record Cell
{
    protected Cell(Location location)
    {
        Location = location;
    }

    public Location Location { get; init; }
}

public record EmptyCell : Cell
{
    public EmptyCell(Location location) : base(location)
    {
    }
}