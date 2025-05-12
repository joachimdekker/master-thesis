using Type = ExcelCompiler.Representations.CodeLayout.TopLevel.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record FunctionCall : Expression
{
    public FunctionCall(string Name, List<Expression> Args) : this(null, Name, Args)
    {
    }

    public FunctionCall(Expression? expression, string name, List<Expression> args, Type? Type = null) : base(Type ?? Type.Derived)
    {
        Object = expression;
        Name = name;
        Args = args;
    }

    public Expression? Object { get; init; }
    public string Name { get; init; }
    public List<Expression> Args { get; init; }
}