namespace ExcelCompiler.Representations.Data.Preview;

public class DataSchema
{
    public string Name { get; init; }
    
    public Dictionary<string, Type> Types { get; init; }
}