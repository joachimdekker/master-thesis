using ExcelCompiler.Domain.CodeLayout.Statements;

namespace ExcelCompiler.Domain.CodeLayout.TopLevel;

public record Method(string Name, Statement[] Body);