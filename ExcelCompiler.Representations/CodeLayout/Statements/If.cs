using System.Diagnostics.CodeAnalysis;
using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record If : Conditional
{
    [SetsRequiredMembers]
    public If(Expression booleanExpression, List<Statement> thenBody, List<Statement>? elseBody = null)
    {
        Condition = booleanExpression;
        Then = thenBody;
        Else = elseBody;
    }

    public Expression Condition { get; init; }
    public List<Statement> Then { get; init; }
    public List<Statement>? Else { get; private set; }

    public override required List<(Expression, List<Statement>)> Cases
    {
        get => [(Condition, Then)];
        init {
            if (value.Count != 1) throw new ArgumentException("If statements can only have one case.");
            (Condition, Then) = value[0];
        }
    }

    public override List<Statement>? Default
    {
        get { return Else ?? new(); }
        set { Else = value; }
    }
}
