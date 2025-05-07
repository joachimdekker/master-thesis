namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record FunctionCall(string Name, Expression[] Args) : Expression;