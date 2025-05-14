using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout;

public record ListOf : Type
{
    public Type ElementType;

    public ListOf(Type type) : base($"List<{type.Name}>)")
    {
        ElementType = type;
    }
};