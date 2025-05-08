namespace ExcelCompiler.Representations.Data;

public abstract record DataSchema
{
    public string Name { get; init; }
}