using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record PropertyAccess(Type Type, Expression Self, string Name) : Expression(Type);