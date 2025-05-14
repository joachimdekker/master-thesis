using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Constant(Type Type, object Value) : Expression(Type);