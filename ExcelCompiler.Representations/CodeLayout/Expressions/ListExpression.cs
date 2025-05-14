using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record ListExpression : Expression
{
    public ListExpression(List<Expression> members, Type? type = null) : base(type ?? members[0].Type)
    {
        Members = members;
    }

    public List<Expression> Members { get; init; }
} 