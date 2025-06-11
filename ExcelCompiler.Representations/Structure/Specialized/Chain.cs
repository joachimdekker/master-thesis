using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

public class Chain : Construct
{
    
    public string Header { get; init; }
    
    public Selection Initialisation { get; init; }
    
    public Selection Data { get; init; }
    
    public Selection Footer { get; init; }
    
    public Dictionary<string, Range> Columns { get; init; } = [];
}