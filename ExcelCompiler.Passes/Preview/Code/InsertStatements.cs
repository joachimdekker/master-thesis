using System.Diagnostics;
using ExcelCompiler.Passes.Code;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;

namespace ExcelCompiler.Passes.Preview.Code;

[CompilerPass]
public class InsertStatements
{
    // Transform Let .. In .. expressions into separate statements
    public Project Transform(Project project)
    {

        var classes = project.Classes.Select(TransformClassMethods);
        
        return project with
        {
            Classes = classes.ToList(),
        };
    }

    private Class TransformClassMethods(Class @class)
    {
        var methods = @class.Methods.Select(Transform);
        return @class with { Methods = methods.ToList() };
    }

    private Method Transform(Method method)
    {
        List<Statement> newBody = [];
        foreach (Statement statement in method.Body)
        {
            switch (statement)
            {
                case Return returnStatement:
                {
                    var transformed = Separate(returnStatement.ReturnExpr);
                    newBody.AddRange([..transformed.Statements, new Return(transformed.Calculations)]);
                    break;
                }
                case ExpressionStatement expressionStatement:
                {
                    var transformed = Separate(expressionStatement.Expression);
                    newBody.AddRange([..transformed.Statements, new ExpressionStatement(transformed.Calculations)]);
                    break;
                }
                default:
                    newBody.Add(statement);
                    break;
            }
        }
        
        return method with { Body = newBody.ToArray() } ;
    }

    private (List<Statement> Statements, Expression Calculations) Separate(Expression expr)
    {
        if (expr is not Let let) return ([], expr);
        
        var expression = Separate(let.Expression);
        var declaration = Separate(let.Assignment.Value);
        
        // Debug.Assert(declaration.Statements.Count == 0, "Let should not have statements");
        
        return expression with
        {
            Statements =
            [
                ..declaration.Statements, 
                new Declaration(let.Assignment.Variable, declaration.Calculations),
                ..expression.Statements,
            ]
        };

    }
}