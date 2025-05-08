namespace ExcelCompiler.Representations.Data;

public record SimpleDataSchema : DataSchema
{
    public Type Type { get; init; }
}