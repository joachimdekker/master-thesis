namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record ListAccessor(Type Type, Expression List, Expression Accessor) : Expression(Type);