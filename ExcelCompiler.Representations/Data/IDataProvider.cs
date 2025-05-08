namespace ExcelCompiler.Representations.Data;

public class SpreadsheetDataProvider
{
    public Guid Id { get; }
    public string RepositoryName { get; }

    private object?[,] _data;
    
    public SpreadsheetDataProvider(Guid id, string repositoryName)
    {
        Id = id;
        RepositoryName = repositoryName;
    }
    
    public object? this[int row, int column]
    {
        get => _data[row, column];
        set => _data[row, column] = value;
    }
    
    public int RowCount => _data.GetLength(0);
    public int ColumnCount => _data.GetLength(1);
    
    public void Resize(int rowCount, int columnCount)
    {
        var @new = new object?[rowCount, columnCount];
        
        for (var i = 0; i < Math.Min(RowCount, rowCount); i++)
        {
            for (var j = 0; j < Math.Min(ColumnCount, columnCount); j++)
            {
                @new[i, j] = _data[i, j];
            }
        }
        
        _data = @new;
    }
}