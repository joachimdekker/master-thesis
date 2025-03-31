using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Domain.Spreadsheet;

public record FormulaCell(Location location, FunctionComposition formula) : Cell(location);