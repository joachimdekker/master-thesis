namespace ExcelCompiler.Representations.Data.Preview.Specialized;

public class TabularInMemoryDataRepository(string name, DataSchema schema) : InMemoryDataRepository(name, schema)
{
    public int Count { get; init; }
    
    public static TabularInMemoryDataRepository FromRows(IEnumerable<IEnumerable<object>> rows, DataSchema schema,
        string name)
    {
        var names = schema.Types.Keys.ToList();
        var repo = new TabularInMemoryDataRepository(name, schema)
        {
            Count = rows.Count()
        };

        // Generate the names for the rows in the format
        // [column_name]_[row_index]
        foreach (var (rowIndex, row) in rows.Index())
        {
            // Iterate over the columns
            foreach (var (columnIndex, value) in row.Index())
            {
                var keyName = $"{names[columnIndex]}_{rowIndex}";
                repo.Add(keyName, value);
            }
        }

        return repo;
    }

    public static TabularInMemoryDataRepository FromColumns(IEnumerable<IEnumerable<object>> columns, DataSchema schema,
        string name)
    {
        var names = schema.Types.Keys.ToList();
        var repo = new TabularInMemoryDataRepository(name, schema)
        {
            Count = columns.First().Count()
        };
        
        // Generate the names for the rows in the format
        // [column_name]_[row_index]
        foreach (var (columnIndex, column) in columns.Index())
        {
            // Iterate over the columns
            foreach (var (rowIndex, value) in column.Index())
            {
                var keyName = $"{names[columnIndex]}_{rowIndex}";
                repo.Add(keyName, value);
            }
        }
        
        return repo;
    }
}