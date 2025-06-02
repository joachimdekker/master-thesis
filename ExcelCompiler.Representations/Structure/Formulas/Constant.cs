namespace ExcelCompiler.Representations.Structure.Formulas;

public record Constant : FormulaExpression
{
    public Constant(object value)
    {
        Value = value;
    }

    public object Value { get; init; }
    
    public Type Type => Value.GetType();
}