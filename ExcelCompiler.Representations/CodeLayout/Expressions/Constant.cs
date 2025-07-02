namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Constant(Type Type, object Value) : Expression(Type)
{
    public Constant(object value)
        : this(new Type(value.GetType()), value)
    {
    }

    public static Expression List(ListOf listType, List<object> values)
    {
        return new ListExpression(values.Select(o => new Constant(listType.ElementType, o)).Cast<Expression>().ToList(), listType.ElementType);
    }
}
