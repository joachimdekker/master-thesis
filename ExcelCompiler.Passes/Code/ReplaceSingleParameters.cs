using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;

namespace ExcelCompiler.Passes.Code;

public static class ReplaceSingleVariablesPass
{
    public static Project ReplaceSingleVariables(this Project project)
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
        Dictionary<Variable, Constant> variables = new();
        
        // Run through the statements
        foreach (var statement in method.Body)
        {
            if (statement is Declaration declaration)
            {
                
            }
        }
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