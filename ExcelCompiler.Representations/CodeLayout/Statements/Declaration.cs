using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Declaration(Variable Variable, Expression DeclarationExpr) : Statement;

public record Assignment(Variable Variable, Expression Expression) : Statement;