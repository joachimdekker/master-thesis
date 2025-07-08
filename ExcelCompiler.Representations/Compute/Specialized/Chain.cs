using ExcelCompiler.Representations.References;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Compute.Specialized;

public record Chain(Range Range) : Construct(Range)
{
    public record ColumnReference(string chainName, string columnName, Location location) : ComputeUnit(location)
    {
        public string ChainName { get; init; } = chainName;
        public string ColumnName { get; init; } = columnName;
    }

    public record CellReference(string chainName, string columnName, Location location) : ComputeUnit(location)
    {
        public string ChainName { get; init; } = chainName;
    }

    public string Name { get; init; }

    public List<ChainColumn> Columns { get; init; } = [];

    public DataReference Data { get; set; }
    
    public ChainStructureData StructureData { get; set; } = new();
}

public abstract record ChainColumn
{
    public required string Name { get; init; }
    public required Type Type { get; set; }

    public required Range Location { get; set; }
}

public record DataChainColumn : ChainColumn
{
    public record Reference(string ChainName, string ColumnName, int Index, Location Location) : ComputeUnit(Location);
}

public record ComputedChainColumn : ChainColumn
{
    public record CellReference(string ChainName, string ColumnName, int Index, Location Location) : ComputeUnit(Location);

    public ComputeUnit? Computation { get; set; }
}

public record RecursiveChainColumn : ChainColumn
{
    public record RecursiveCellReference(string ChainName, string ColumnName, int Recursion, Location Location) : ComputeUnit(Location);

    public ComputeUnit? Initialization { get; set; }

    public ComputeUnit? Computation { get; set; }

    public ComputeUnit? Footer { get; set; }
    
    public int NoBaseCases { get; set; }
}
