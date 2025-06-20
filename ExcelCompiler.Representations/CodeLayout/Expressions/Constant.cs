namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Constant(Type Type, object Value) : Expression(Type)
{
    public Constant(object value)
        : this(new Type(value.GetType()), value)
    {
    }
}
