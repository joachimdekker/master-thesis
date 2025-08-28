namespace ExcelCompiler.Evaluation.Models;

public class TrialRun
{
    public required Trial Trial { get; init; }
    
    public required Dictionary<CellReference, object> Inputs { get; init; }
    
    public required Dictionary<CellReference, object> Results { get; init; }
}