using ExcelCompiler.Representations.CodeLayout.Statements;

namespace ExcelCompiler.Representations.CodeLayout.TopLevel;

public record Method(string Name, Statement[] Body);