using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Domain.Spreadsheet;

public record FormulaCell(Location location, Function formula) : Cell(location);