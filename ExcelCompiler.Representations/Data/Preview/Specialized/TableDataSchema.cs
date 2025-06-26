namespace ExcelCompiler.Representations.Data.Preview.Specialized;

public class TableDataSchema : DataSchema
{
    public Dictionary<string, Type> Columns { get; init; }
}