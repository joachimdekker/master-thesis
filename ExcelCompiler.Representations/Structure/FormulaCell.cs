namespace ExcelCompiler.Representations.Structure;

public record FormulaCell(Location Location, Type Type, string Formula) : Cell(Location, Type);