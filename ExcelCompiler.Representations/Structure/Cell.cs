namespace ExcelCompiler.Domain.Structure;

public abstract record Cell
{
    protected Cell(Location location)
    {
        Location = location;
    }

    public Location Location { get; init; }
}