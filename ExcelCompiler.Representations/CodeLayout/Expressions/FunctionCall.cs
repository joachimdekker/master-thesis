namespace ExcelCompiler.Domain.CodeLayout.Expressions;

public record FunctionCall(string Name, Expression[] Args) : Expression;