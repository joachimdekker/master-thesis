namespace ExcelCompiler.Representations.Structure.Formulas;

public abstract record FormulaTraverser<TRes>
{
    protected abstract TRes Constant(Constant constant);
    
    protected abstract TRes Function(Function function, List<TRes> arguments);
    
    protected abstract TRes Operator(Operator @operator, List<TRes> arguments);
    
    protected abstract TRes CellReference(CellReference reference);
    
    protected abstract TRes RangeReference(RangeReference reference);
    
    protected abstract TRes TableReference(TableReference reference);

    public TRes Traverse(FormulaExpression expression)
    {
        return expression switch
        {
            Constant constant => Constant(constant),
            Operator @operator => Operator(@operator, @operator.Arguments.Select(Traverse).ToList()),
            Function function => Function(function, function.Arguments.Select(Traverse).ToList()),
            CellReference reference => CellReference(reference),
            RangeReference reference => RangeReference(reference),
            TableReference reference => TableReference(reference),
            _ => throw new InvalidOperationException($"Expression type {expression.GetType()} is not supported.")
        };
    }
}

public abstract record FormulaTransformer : FormulaTraverser<FormulaExpression>
{
    protected override FormulaExpression Constant(Constant constant) => constant;
    
    protected override FormulaExpression Function(Function function, List<FormulaExpression> arguments)
        => function with
        {
            Arguments = arguments,
        };
    
    protected override FormulaExpression Operator(Operator @operator, List<FormulaExpression> arguments)
        => @operator with
        {
            Arguments = arguments,
        };
    
    protected override FormulaExpression CellReference(CellReference reference) => reference;
    
    protected override FormulaExpression RangeReference(RangeReference reference) => reference;
    
    protected override FormulaExpression TableReference(TableReference reference) => reference;
}

public record CollectionFormulaTraverser<TRes> : FormulaTraverser<List<TRes>>
{
    protected override List<TRes> Constant(Constant constant) => [];
    
    protected override List<TRes> Function(Function function, List<List<TRes>> arguments) => arguments.SelectMany(x => x).ToList();
    
    protected override List<TRes> Operator(Operator @operator, List<List<TRes>> arguments) => arguments.SelectMany(x => x).ToList();
    
    protected override List<TRes> CellReference(CellReference reference) => [];
    
    protected override List<TRes> RangeReference(RangeReference reference) => [];
    
    protected override List<TRes> TableReference(TableReference reference) => [];
}