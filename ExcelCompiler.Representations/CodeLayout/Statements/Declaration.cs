using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Declaration(Variable Variable, Expression Expression) : Statement;