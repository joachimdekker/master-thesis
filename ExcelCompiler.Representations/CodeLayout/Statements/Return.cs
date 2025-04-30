using ExcelCompiler.Domain.CodeLayout.Expressions;

namespace ExcelCompiler.Domain.CodeLayout.Statements;

public record Return(Expression ReturnExpr) : Statement;