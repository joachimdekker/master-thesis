using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Domain.Structure;

public record FormulaCell(Location location, FunctionComposition formula) : Cell(location);