using ExcelCompiler.Representations.CodeLayout.Expressions;
using Type = ExcelCompiler.Representations.CodeLayout.TopLevel.Type;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Declaration(Variable Variable, Expression DeclarationExpr) : Statement;

public record Assignment(Variable Variable, Expression Expression) : Statement;