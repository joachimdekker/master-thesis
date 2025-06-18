using ExcelCompiler.Representations.Structure;
using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public record Nil(Location location) : ComputeUnit(location);