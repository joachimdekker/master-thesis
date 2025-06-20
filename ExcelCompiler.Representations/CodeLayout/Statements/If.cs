using System.Diagnostics.CodeAnalysis;
using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record If : Conditional
{
    [SetsRequiredMembers]
    public If(Expression booleanExpression, List<Statement> thenBody)
    {
        Condition = booleanExpression;
        Then = thenBody;
    }

    public Expression Condition { get; }
    public List<Statement> Then { get; }
    public List<Statement>? Else { get; }

    public override required List<(Expression, List<Statement>)> Cases
    {
        get => [(Condition, Then)];
        init => throw new InvalidOperationException("To set the cases of the IF statement, use the constructor");
    }

    public override List<Statement> Default => Else ?? new();
}
