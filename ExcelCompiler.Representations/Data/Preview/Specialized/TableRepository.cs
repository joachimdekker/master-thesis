namespace ExcelCompiler.Representations.Data.Preview.Specialized;

public class TableRepository : IDataRepository
{
    public string Name { get; init; }
    
    public int Count { get; set; }
    
    DataSchema IDataRepository.Schema
    {
        get => Schema; 
        init => Schema = value as TableDataSchema ?? throw new ArgumentException("Schema must be of type ChainDataSchema");
    }
    
    public TableDataSchema Schema { get; set; }

    public Dictionary<string, List<object>> Columns { get; init; }

    public IEnumerable<Dictionary<string, object>> GetRows()
    {
        var columnNames = Columns.Keys.ToList();
        
        for (var i = 0; i < Count; i++)
        {
            yield return columnNames.ToDictionary(name => name, name => Columns[name][i]);
        }
    }
}