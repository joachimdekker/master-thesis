using ExcelCompiler.Representations.References;

namespace ExcelCompiler.Representations.Compute.Specialized;

public record ColumnOperation(string StructureName, string ColumnName, Location Location) : ComputeUnit(Location);

public record FooterReference(string StructureName, string ColumnName, Location Location) : ComputeUnit(Location);

public record RecursiveResultReference(string StructureName, string ColumnName, Location Location) : ComputeUnit(Location);

public record ConstructCellReference(string StructureName, string Columnname, int Row, Location Location)
    : ComputeUnit(Location);
