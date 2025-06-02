using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record FunctionCall : Expression
{
    public FunctionCall(string Name, List<Expression> Arguments) : this(null, Name, Arguments)
    {
    }

    public FunctionCall(Expression? expression, string name, List<Expression> arguments, Type? Type = null) : base(Type ?? Type.Derived)
    {
        Object = expression;
        Name = name;
        Arguments = arguments;
    }

    public Expression? Object { get; init; }
    public string Name { get; init; }
    public List<Expression> Arguments { get; init; }
}