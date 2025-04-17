namespace ExcelCompiler.Domain.Structure;

public record ValueCell<T>(Location Location, T Value) : Cell(Location);