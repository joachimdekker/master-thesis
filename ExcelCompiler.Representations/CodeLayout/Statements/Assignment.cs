using ExcelCompiler.Domain.CodeLayout.Expressions;

namespace ExcelCompiler.Domain.CodeLayout.Statements;

public record Assignment(string Variable, Type Type, Expression DeclarationExpr) : Statement;