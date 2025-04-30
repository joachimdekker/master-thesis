namespace ExcelCompiler.Domain.CodeLayout.Expressions;

public record Constant(Type Type, object Value) : Expression;