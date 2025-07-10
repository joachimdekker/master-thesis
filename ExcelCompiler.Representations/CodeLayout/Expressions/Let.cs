namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Let(Assignment Assignment, Expression Expression) : Expression(Expression.Type)
{
    
}