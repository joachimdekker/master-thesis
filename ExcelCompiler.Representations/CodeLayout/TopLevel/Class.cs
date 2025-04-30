namespace ExcelCompiler.Domain.CodeLayout.TopLevel;

// Top-level code structure
public record Class(string Name, Property[] Members, Method[] Methods);

// Abstract base class for all statements

// Abstract base class for all expressions