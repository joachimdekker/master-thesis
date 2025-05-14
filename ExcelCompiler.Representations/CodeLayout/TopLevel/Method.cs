using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;

namespace ExcelCompiler.Representations.CodeLayout.TopLevel;

public record Method
{
    public Method(string Name, Variable[] Parameters, Statement[] Body) : this(GenerateType(Body), Name, Parameters, Body)
    {
    }

    private static Type GenerateType(Statement[] body)
    {
        if (body.Length == 0) return Type.Void;
        
        Statement lastStatement = body[^1];

        return lastStatement switch
        {
            Return returnStatement when returnStatement.ReturnExpr.Type == Type.Derived => new("double"),
            Return returnStatement => returnStatement.ReturnExpr.Type,
            _ => Type.Void
        };
    }

    public Method(Type Type, string Name, Variable[] Parameters, Statement[] Body)
    {
        if (Type == Type.Derived)
        {
            throw new ArgumentException("Cannot create a method with a derived type");
        }
        
        this.Type = Type;
        this.Name = Name;
        this.Parameters = Parameters;
        this.Body = Body;
    }

    public Type Type { get; init; }
    public string Name { get; init; }
    public Variable[] Parameters { get; init; }
    public Statement[] Body { get; init; }
}