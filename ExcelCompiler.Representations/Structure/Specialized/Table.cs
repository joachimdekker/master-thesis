using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

public class Table : Construct
{
    public Selection? Header { get; init; }
    
    public Selection Data { get; init; }
    
    public Selection? Footer { get; init; }
    
    public Dictionary<string, LineSelection> Columns { get; init; } = [];
}