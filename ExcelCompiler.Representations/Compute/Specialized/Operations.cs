using ExcelCompiler.Representations.References;

namespace ExcelCompiler.Representations.Compute.Specialized;

public record ColumnOperation(Construct Structure, string ColumnName, Location Location) : ComputeUnit(Location);

public record FooterReference(string StructureName, string ColumnName, Location Location) : ComputeUnit(Location);

/// <summary>
/// Represents a reference to a cell in a recursive column of a chain.
/// </summary>
/// <param name="StructureName"></param>
/// <param name="ColumnName"></param>
/// <param name="Location"></param>
/// <param name="Row"></param>
public record RecursiveResultReference(string StructureName, string ColumnName, int Row, Location Location) : ComputeUnit(Location);

public record ConstructCellReference(string StructureName, string Columnname, int Row, Location Location)
    : ComputeUnit(Location);
