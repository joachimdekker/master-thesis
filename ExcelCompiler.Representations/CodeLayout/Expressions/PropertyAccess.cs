namespace ExcelCompiler.Domain.CodeLayout.Expressions;

public record PropertyAccess(Type Type, object Self, string Name) : Expression;