namespace ExcelCompiler.Representations.Data.Preview;

public class InMemoryDataRepository(string name, DataSchema schema) : IDataRepository
{
    public string Name { get; init; } = name;
    public DataSchema Schema { get; init; } = schema;
    private Dictionary<string, object> Data { get; set; }
    
    public object this[string key] => Data[key];
    
    public void Add(string key, object value) => Data[key] = value;
    public void Remove(string key) => Data.Remove(key);
}