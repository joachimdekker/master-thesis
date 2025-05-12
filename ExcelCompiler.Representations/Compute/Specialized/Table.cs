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
    public class CellReference(string TableName, string ColumnName, Location location) : ComputeUnit(location);

    public class ColumnReference(string TableName, string ColumnName, Location location) : ComputeUnit(location);
    
    public enum TabelColumnType
    {
        Data,
        Computed
    }
    
    public Type Type { get; init; }
    
    public required string Name { get; init; }
    
    public ComputeUnit? Computation { get; init; }

    public required TabelColumnType ColumnType;
}