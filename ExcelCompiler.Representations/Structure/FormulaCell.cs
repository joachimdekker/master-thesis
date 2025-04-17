using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Domain.Structure;

public record FormulaCell(Location Location, Function ComputePlan) : Cell(Location);