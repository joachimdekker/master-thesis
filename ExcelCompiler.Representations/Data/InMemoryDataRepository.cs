namespace ExcelCompiler.Representations.Data;

public class InMemoryDataRepository : IDataRepository
{
    public Guid Id => Guid.NewGuid();
    public string Name { get; init; }
    public DataSchema Schema { get; init; }
    public ProviderType Provider { get; init; }
    
    private object[,] Data { get; set; }
    
    public InMemoryDataRepository(string name, DataSchema schema, object[,] data)
    {
        Name = name;
        Schema = schema;
        Provider = ProviderType.InMemory;
        
        // Generate the data form based on the schema.
        // TODO: Implement a checker that checks the schema and typing of the data.
        Data = data;

    }
    
    // TODO: Implement a beter version of the repository without a reference to tabular data.
    public IEnumerable<object> GetDataFromColumn(string columnName)
    {
        // Get the index of the column based on the schema
        ColumnarDataSchema schema = Schema as ColumnarDataSchema ?? throw new InvalidOperationException("Can only use this method with columnar data schema.");
        int columnIndex = schema.Properties.Keys.ToList().IndexOf(columnName);
        
        // Get everything from the column
        for (int i = 0; i < Data.GetLength(0); i++)
        {
            yield return Data[i, columnIndex];
        }
    }
    
    public IEnumerable<object> GetDataFromRow(int row)
    {
        for (int i = 0; i < Data.GetLength(1); i++)
        {
            yield return Data[row, i];
        }
    }

    public IEnumerable<object[]> GetDataFromRows(int startRow = 0, int endRow = -1)
    {
        endRow = endRow <= -1 ? Data.GetLength(0) + endRow : endRow;

        for (int i = startRow; i <= endRow; i++)
        {
            yield return GetDataFromRow(i).ToArray();
        }
    }
}