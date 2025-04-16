namespace ExcelCompiler.Domain.Structure;

public abstract record Cell
{
    protected Cell(Location location, List<Cell>? dependencies = null)
    {
        Location = location;
        Dependencies = dependencies ?? [];
    }

    public Location Location { get; init; }
    
    public List<Cell> Dependencies { get; init; }
}