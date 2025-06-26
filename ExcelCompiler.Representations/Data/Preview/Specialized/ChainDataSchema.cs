namespace ExcelCompiler.Representations.Data.Preview.Specialized;

public class ChainDataSchema : DataSchema
{
    public Dictionary<string, Type> Initialization { get; init; }
    
    public Dictionary<string, Type> Data { get; init; }
}