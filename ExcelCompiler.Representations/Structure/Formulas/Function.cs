namespace ExcelCompiler.Representations.Structure.Formulas;

public record Function : FormulaExpression
{
    public Function(string name, List<FormulaExpression> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
    
    public string Name { get; init; }
    
    public List<FormulaExpression> Arguments { get; init; } = new();
}