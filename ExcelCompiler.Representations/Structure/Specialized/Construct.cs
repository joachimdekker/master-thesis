using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

/// <summary>
/// Describes a special structure within an Excel Spreadsheet.
/// </summary>
public class Construct
{
    public required string Name { get; set; }
    
    public required Range Location { get; set; }

    public bool IsInput = false;
}