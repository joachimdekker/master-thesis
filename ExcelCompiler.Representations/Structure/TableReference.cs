namespace ExcelCompiler.Domain.Structure;

public record TableReference : Reference
{
    public string TableName { get; init; }
    public string ColumnName { get; init; }
}