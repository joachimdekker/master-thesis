using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Variable(string Name, Type Type) : Expression(Type)
{
    public Variable(string Name) : this(Name, Type.Derived) {}
}