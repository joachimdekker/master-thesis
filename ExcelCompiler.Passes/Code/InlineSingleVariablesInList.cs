using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;

namespace ExcelCompiler.Passes.Code;

public static class InlineSingleVariablesInList
{
    public static Project RemoveSingleVariables(this Project project)
    {
        // Loop through all classes
        foreach (var @class in project.Classes)
        {
            RemoveSingleVariables(@class);
        }

        return project;
    }

    public static void RemoveSingleVariables(Class @class)
    {
        foreach (var method in @class.Methods)
        {
            RemoveSingleVariables(method);
        }
    }

    public static void RemoveSingleVariables(Method method)
    {
        Dictionary<Variable, Expression> variables = new();
        Dictionary<Variable, int> variableUses = new();
        Dictionary<Variable, int> variableAssignments = new();
        
        // Run through the statements
        foreach (var statement in method.Body)
        {
            if (statement is Declaration declaration)
            {
                variables[declaration.Variable] = declaration.Expression;
                variableUses[declaration.Variable] = 0;
                variableAssignments[declaration.Variable] = 1;

                GetVariableCount(declaration.Expression, variableUses);
            }
            
            if (statement is Assignment assignment)
            {
                variableAssignments[assignment.Variable]++;
                
                GetVariableCount(assignment.Expression, variableUses);
            }
            
            if (statement is Return @return)
            {
                GetVariableCount(@return.ReturnExpr, variableUses);
            }

            if (statement is ExpressionStatement expressionStatement)
            {
                GetVariableCount(expressionStatement.Expression, variableUses);
            }
        }
        
        // Remove single list shit things
        foreach (var statement in method.Body)
        {
            if (statement is Declaration declaration)
            {
                RemoveSingleVariables(declaration.Expression, variables, variableUses);
            }
        }
    }

    private static void RemoveSingleVariables(Expression declarationExpression, Dictionary<Variable, Expression> variables, Dictionary<Variable, int> variableUses)
    {
        
    }

    private static void GetVariableCount(Expression declarationDeclarationExpr, Dictionary<Variable, int> variableCounts)
    {
        switch (declarationDeclarationExpr)
        {
            case Variable variable:
                variableCounts[variable]++;
                break;
            case FunctionCall functionCall:
                foreach (var argument in functionCall.Arguments)
                {
                    GetVariableCount(argument, variableCounts);
                }
                break;
            case ListExpression listExpression:
                foreach (var argument in listExpression.Members)
                {
                    GetVariableCount(argument, variableCounts);
                }

                break;
            case Lambda lambda:
                GetVariableCount(lambda.Body, variableCounts);
                break;
            case ObjectCreation objectCreation:
                foreach (var objectCreationArgument in objectCreation.Arguments)
                {
                    GetVariableCount(objectCreationArgument, variableCounts);
                }

                break;
            case PropertyAccess propertyAccess:
                GetVariableCount(propertyAccess.Self, variableCounts);
                break;
        }
    }
}