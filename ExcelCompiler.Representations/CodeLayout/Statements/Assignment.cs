using ExcelCompiler.Representations.CodeLayout.Expressions;
using Type = ExcelCompiler.Representations.CodeLayout.TopLevel.Type;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Assignment(Variable Variable, Type Type, Expression DeclarationExpr) : Statement;