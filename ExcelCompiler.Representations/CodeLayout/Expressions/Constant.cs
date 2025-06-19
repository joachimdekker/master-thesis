namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Constant(Type Type, object Value) : Expression(Type);
