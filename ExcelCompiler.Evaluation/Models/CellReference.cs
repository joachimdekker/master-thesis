namespace ExcelCompiler.Evaluation.Models;

public sealed record CellReference(string? SheetName, string Address)
{
    public string AddressWithSheet => SheetName is not null ? $"'{SheetName}'!{Address}" : Address;
    
    public static implicit operator string(CellReference cr) => cr.AddressWithSheet;
}