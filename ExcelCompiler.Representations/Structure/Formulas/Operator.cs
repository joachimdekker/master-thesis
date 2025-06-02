namespace ExcelCompiler.Representations.Structure.Formulas;

public record Operator : Function
{
    protected Operator(string name, List<FormulaExpression> arguments) : base(name, arguments) {}
    
    public static Operator Plus(FormulaExpression left, FormulaExpression right) => new("+", [left, right]);
    
    public static Operator Minus(FormulaExpression left, FormulaExpression right) => new("-", [left, right]);
    
    public static Operator Multiply(FormulaExpression left, FormulaExpression right) => new("*", [left, right]);
    
    public static Operator Divide(FormulaExpression left, FormulaExpression right) => new("/", [left, right]);
    
    public static Operator Power(FormulaExpression left, FormulaExpression right) => new("^", [left, right]);
    
    public static Operator Modulo(FormulaExpression left, FormulaExpression right) => new("%", [left, right]);
    
    public static Operator Negate(FormulaExpression operand) => new("-", [operand]);
}