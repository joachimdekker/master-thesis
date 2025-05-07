using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Return(Expression ReturnExpr) : Statement;