namespace ExcelCompiler.Domain.Spreadsheet;

public abstract record Cell(Location location, List<Cell>? dependencies = null)
{
    public List<Cell> Dependencies => dependencies ?? [];
}