namespace ExcelCompiler.Representations.Data.Preview.Specialized;

public class ChainRepository : IDataRepository
{
    public ChainRepository(string name, ChainDataSchema schema, Dictionary<string, List<object>> initialization, Dictionary<string, List<object>> data)
    {
        Name = name;
        Schema = schema;
        
        // Validate the initialization and data.
        if (!schema.Initialization.Keys.All(initialization.ContainsKey) || !schema.Data.Keys.All(data.ContainsKey))
        {
            throw new ArgumentException("Initialization and data must contain all the properties of the schema.");
        }
        
        Initialization = initialization;
        Data = data;
        
        Count = data.First().Value.Count;
    }

    public string Name { get; init; }

    public Dictionary<string, List<object>> Initialization { get; set; }
    
    public Dictionary<string, List<object>> Data { get; set; }
    
    DataSchema IDataRepository.Schema
    {
        get => Schema; 
        init => Schema = value as ChainDataSchema ?? throw new ArgumentException("Schema must be of type ChainDataSchema");
    }
    
    public ChainDataSchema Schema { get; set; }
    
    public int Count { get; set; }
    
    public IEnumerable<Dictionary<string, object>> GetRows()
    {
        var columnNames = Data.Keys.ToList();
        
        for (var i = 0; i < Count; i++)
        {
            yield return columnNames.ToDictionary(name => name, name => Data[name][i]);
        }
    }
    
}