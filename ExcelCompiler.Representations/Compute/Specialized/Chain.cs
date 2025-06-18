using ExcelCompiler.Representations.References;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Compute.Specialized;

public class Chain(Range range) : Construct(range)
{
    public class ColumnReference(string chainName, string columnName, Location location) : ComputeUnit(location)
    {
        public string ChainName { get; init; } = chainName;
        public string ColumnName { get; init; } = columnName;
    }
    
    public class CellReference(string chainName, string columnName, Location location) : ComputeUnit(location)
    {
        public string ChainName { get; init; } = chainName;
    }
    
    public string Name { get; init; }
    
    public List<ChainColumn> Columns { get; init; } = [];
    
    public DataReference Data { get; set; }
}

public abstract record ChainColumn
{
    public required string Name { get; init; }
    public required Type Type { get; set; }
}

public record DataChainColumn : ChainColumn
{
    
}

public record ComputedChainColumn : ChainColumn
{
    public ComputeUnit? Computation { get; set; }
}

public record RecursiveChainColumn : ChainColumn
{
    public class RecursiveCellReference(string ColumnName, int Recursion, Location location) : ComputeUnit(location);
    
    public ComputeUnit? Initialization { get; set; }
    
    public ComputeUnit? Computation { get; set; }
    
    public ComputeUnit? Footer { get; set; }
}