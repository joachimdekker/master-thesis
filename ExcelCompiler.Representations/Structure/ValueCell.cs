namespace ExcelCompiler.Domain.Structure;

public record ValueCell(Location location, string value) : Cell(location);