using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Assignment(string Variable, Type Type, Expression DeclarationExpr) : Statement;