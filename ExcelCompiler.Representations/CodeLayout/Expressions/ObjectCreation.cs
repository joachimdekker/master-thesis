using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record ObjectCreation(Type Type, List<Expression> Arguments) : Expression(Type);