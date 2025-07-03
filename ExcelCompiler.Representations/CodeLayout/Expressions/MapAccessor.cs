namespace ExcelCompiler.Representations.CodeLayout.Expressions;

public record MapAccessor : Expression
{
    public MapAccessor(Expression Map, Expression Accessor) : base((Map.Type as Map)!.Range)
    {
        this.Map = Map;
        this.Accessor = Accessor;
    }

    public Expression Map { get; init; }
    public Expression Accessor { get; init; }

    public void Deconstruct(out Expression Map, out Expression Accessor)
    {
        Map = this.Map;
        Accessor = this.Accessor;
    }
}