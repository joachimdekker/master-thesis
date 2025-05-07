using ExcelCompiler.Domain.Structure;

namespace ExcelCompiler.Representations.Structure;

public record TableReference : Reference
{
    public string TableName { get; init; }
    public string ColumnName { get; init; }
}