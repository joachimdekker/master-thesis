using ExcelCompiler.Representations.Structure;
using Location = ExcelCompiler.Representations.References.Location;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Compute.Specialized;

public record Table : Construct
{
    public record ColumnReference(string tableName, string columnName, Location location) : ComputeUnit(location)
    {
        public string TableName { get; init; } = tableName;
        public string ColumnName { get; init; } = columnName;
    }

    public record CellReference(string tableName, string columnName, Location location) : ComputeUnit(location)
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
    public record CellReference(string TableName, string ColumnName, Location location) : ComputeUnit(location)
    {
    }

    public record ColumnReference(string TableName, string ColumnName, Location location) : ComputeUnit(location)
    {
    }

    public enum TableColumnType
    {
        Data,
        Computed
    }

    public Type Type { get; init; }

    public Range Location { get; init; }

    public required string Name { get; init; }

    public ComputeUnit? Footer { get; set; }

    public ComputeUnit? Computation { get; set; }

    public required TableColumnType ColumnType;
}
