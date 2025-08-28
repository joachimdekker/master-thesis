using ExcelCompiler.Evaluation.Utils;

namespace ExcelCompiler.Evaluation.Config;

public sealed class AppConfig
{
    public required string SpreadsheetsRoot { get; init; }
    
    public required string ToolProject  { get; init; }
    
    public int TrialsPerWorkbook { get; init; } = 5;
    
    public int TestsPerTrial    { get; init; } = 1000;
    
    public double NumericTolerance { get; init; } = 1e-6;
    
    public int? RandomSeed { get; init; } = 42;
}