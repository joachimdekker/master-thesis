using ExcelCompiler.Representations.Structure;
using Location = ExcelCompiler.Representations.References.Location;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Compute.Specialized;

public class Table : Construct
{ 
    public class ColumnReference(string tableName, string columnName, Location location) : ComputeUnit(location)
    {
        public string TableName { get; init; } = tableName;
        public string ColumnName { get; init; } = columnName;
    }

    public class CellReference(string tableName, string columnName, Location location) : ComputeUnit(location)
    {
        public string TableName { get; init; } = tableName;
    }
    
    public string Name { get; }
    
    public List<TableColumn> Columns { get; init; } = [];
    
    public DataReference Data { get; set; }
    
    public Range Location { get; init; }
    
    public Table(string name, Range location, DataReference data) : base(location)
    {
        Name = name;
        Data = data;
        Location = location;
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
    
    public ComputeUnit? Footer { get; set; }
    
    public ComputeUnit? Computation { get; set; }

    public required TableColumnType ColumnType;
}