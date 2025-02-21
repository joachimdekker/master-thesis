namespace ExcelCompiler.Domain.Spreadsheet;

public record ValueCell(Location location, string value) : Cell(location);