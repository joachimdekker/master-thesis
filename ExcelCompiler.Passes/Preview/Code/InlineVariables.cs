using ExcelCompiler.Passes.Code;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Passes.Preview.Code;

[CompilerPass]
public class InlineVariables
{
    public Project Transform(Project project)
    {
        var transformer = new InlineVariableTransformer();
        return transformer.Transform(project);
    }
}


file record InlineVariableTransformer : UnitProjectTransformer
{
    protected override Expression Let(Let let, Expression exp, Expression expression)
    {
        if (exp is not Assignment assignment) return base.Let(let, exp, expression);
        
        switch (assignment.Value)
        {
            case Constant constant:
                expression = Replace(expression, assignment.Variable, constant);
                break;
            
            case Variable variable:
                expression = Replace(expression, assignment.Variable, variable);
                break;
            
            default:
                return base.Let(let, assignment, expression);
        }

        return expression;
    }

    private Expression Replace(Expression expression, Variable variable, Expression replacement)
    {
        if (expression is Variable v && v == variable) return replacement;

        return expression switch
        {
            FunctionCall functionCall => FunctionCall(functionCall,
                functionCall.Arguments.Select(a => Replace(a, variable, replacement)).ToList()),
            ListExpression listExpression => ListExpression(listExpression,
                listExpression.Members.Select(a => Replace(a, variable, replacement)).ToList()),
            ListAccessor listAccessor => ListAccessor(listAccessor, Replace(listAccessor.List, variable, replacement),
                Replace(listAccessor.Accessor, variable, replacement)),
            MapAccessor mapAccessor => MapAccessor(mapAccessor, Replace(mapAccessor.Map, variable, replacement),
                Replace(mapAccessor.Accessor, variable, replacement)),
            Lambda lambda => Lambda(lambda, Replace(lambda.Body, variable, replacement)),
            ObjectCreation objectCreation => ObjectCreation(objectCreation,
                objectCreation.Arguments.Select(a => Replace(a, variable, replacement)).ToList()),
            PropertyAccess propertyAccess => PropertyAccess(propertyAccess,
                Replace(propertyAccess.Self, variable, replacement)),
            Assignment assignment => Assignment(assignment, Replace(assignment.Value, variable, replacement)),
            Let let => Let(let, Replace(let.Assignment, variable, replacement),
                Replace(let.Expression, variable, replacement)),
            _ => expression,
        };
    }
}