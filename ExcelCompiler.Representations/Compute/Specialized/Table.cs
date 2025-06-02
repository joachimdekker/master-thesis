using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.Structure.Range;

namespace ExcelCompiler.Representations.Compute.Specialized;

public class Table
{
    public string Name { get; }
    
    public List<TableColumn> Columns { get; init; } = [];
    
    public DataReference Data { get; set; }
    
    public Range Location { get; init; }
    
    public Table(string name, DataReference data)
    {
        Name = name;
        Data = data;
    }
}

public record TableColumn
{
    public class CellReference(string tableName, string columnName, Location location) : ComputeUnit(location)
    {
        public string TableName { get; init; } = tableName;
        public string ColumnName { get; init; } = columnName;
    }

    public class ColumnReference(string tableName, string columnName, Location location) : ComputeUnit(location)
    {
        public string TableName { get; init; } = tableName;
        public string ColumnName { get; init; } = columnName;
    }

    public enum TableColumnType
    {
        Data,
        Computed
    }
    
    public Type Type { get; init; }
    
    public required string Name { get; init; }
    
    public ComputeUnit? Computation { get; init; }

    public required TableColumnType ColumnType;
}