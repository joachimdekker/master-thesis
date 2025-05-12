using Type = ExcelCompiler.Representations.CodeLayout.TopLevel.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record ObjectCreation(Type Type, List<Expression> Arguments) : Expression(Type);