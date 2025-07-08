namespace ExcelCompiler.Representations.Compute;

public class StructureData
{
    public string StructureId { get; set; }
    public List<ExternalConstant> Constants { get; set; } = new();
}

public class ExternalConstant
{
    public string Name { get; set; }
    public ComputeUnit Value { get; set; }
}

public class ChainStructureData : StructureData
{
    public List<ColumnData> Initialisations { get; set; } = new();
    public List<ColumnData> Columns { get; set; } = new();
}

public class ColumnData
{
    public string ColumnId { get; set; }
    public Type Type { get; set; }
    public List<ComputeUnit> Data { get; set; } = new();
}

public class TableStructureData : StructureData
{
    public List<ColumnData> Columns { get; set; } = new();
}