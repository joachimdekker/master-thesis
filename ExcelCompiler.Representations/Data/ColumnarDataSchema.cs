namespace ExcelCompiler.Representations.Data;

public record ColumnarDataSchema : DataSchema
{
    public List<KeyValuePair<string, Type>> Properties { get; init; }
}