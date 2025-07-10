namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Assignment(Variable Variable, Expression Value) : Expression(Value.Type)
{
    
}