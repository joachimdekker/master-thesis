namespace ExcelCompiler.Representations.Structure;

public record FormulaCell(Location Location, string Formula) : Cell(Location);