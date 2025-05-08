namespace ExcelCompiler.Representations.Data;

public record ColumnarDataSchema : DataSchema
{
    public Dictionary<string, Type> Properties { get; init; }
}