namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record Lambda : Expression
{
    public Lambda(List<Variable> parameters, Expression body) : base(body.Type)
    {
        Body = body;
        Parameters = parameters;
    }

    public Expression Body { get; init; }
    
    public List<Variable> Parameters { get; init; }
}