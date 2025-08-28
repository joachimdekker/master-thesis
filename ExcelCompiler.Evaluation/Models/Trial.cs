namespace ExcelCompiler.Evaluation.Models;

public record Trial(string SpreadsheetUrl, List<CellReference> InputCells, List<CellReference> OutputCells);