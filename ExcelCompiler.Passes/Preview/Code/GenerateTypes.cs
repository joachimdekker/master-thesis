using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.Data.Preview;
using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Passes.Preview.Code;

public class GenerateTypes
{
    public List<Class> Generate(ComputeGraph graph, DataManager dataManager)
    {
        var types =
            from construct in graph.Constructs
            let type = construct switch
            {
                Table table => Generate(table),
                Chain chain => Generate(chain),
                _ => throw new InvalidOperationException()
            }
            select type;

        return types.ToList();
    }

    public Class Generate(Chain chain)
    {
        var typeTransformer = new TypeTransformer();

        var properties = chain.Columns.OfType<DataChainColumn>().Select(c => new Property(c.Name, new Type(c.Type))).ToList();
        var computedProperties = chain.Columns
            .OfType<ComputedChainColumn>()
            .Select(c => new Property(c.Name, new Type(c.Type))
            {
                Getter = typeTransformer.Transform(c.Computation!),
            })
            .ToList();

        var chainProperties = chain.Columns
            .OfType<RecursiveChainColumn>()
            .Select(c =>
            {
                Variable counter = new Variable("counter", new Type(typeof(int)));
                Statement[] body = GenerateRecursiveBody(c, counter);

                return new Method(c.Name + "At", [counter], body);
            })
            .ToList();

        return new(chain.Name, [..properties, ..computedProperties], chainProperties);
    }

    private Statement[] GenerateRecursiveBody(RecursiveChainColumn recursiveChainColumn, Variable counter)
    {
        RecursiveTypeTransformer transformer = new(counter);
        Statement baseCase = new If(new FunctionCall("Equals", [counter, new Constant(new Type(typeof(int)), 0)]), [new Return(transformer.Transform(recursiveChainColumn.Initialization!))]);

        Statement recursiveCase = new Return(transformer.Transform(recursiveChainColumn.Computation!));

        return [baseCase, recursiveCase];
    }

    public Class Generate(Table table)
    {
        var typeTransformer = new TypeTransformer();

        var properties = table.Columns.Where(tc => tc.Computation is null).Select(tc => new Property(tc.Name, new Type(tc.Type))).ToList();
        var computedProperties = table.Columns.Where(tc => tc.Computation is not null).Select(tc => new Property(tc.Name, new Type(tc.Type))
        {
            Getter = typeTransformer.Transform(tc.Computation!),
        }).ToList();

        return new(table.Name, [..properties, ..computedProperties], []);
    }
}

file record TypeTransformer() : ComputeGraphTransformer<Expression, Expression>
{
    protected override Expression CellReference(CellReference cellReference, IEnumerable<Expression> dependencies)
        => dependencies.Single();

    protected override Expression RangeReference(RangeReference rangeReference, IEnumerable<Expression> dependencies)
        => new ListExpression(dependencies.ToList());

    protected override Expression TableReference(TableReference tableReference, IEnumerable<Expression> dependencies)
        => throw new InvalidOperationException();

    protected override Expression DataReference(DataReference dataReference, IEnumerable<Expression> dependencies)
        => throw new NotImplementedException();

    protected override Expression Function(Function function, IEnumerable<Expression> dependencies)
        => new FunctionCall(function.Name, dependencies.ToList());
    protected override Expression Constant<T>(ConstantValue<T> constant, IEnumerable<Expression> dependencies)
        => new Constant(new Type(typeof(T)), constant.Value!);

    protected override Expression Nil(Nil nil, IEnumerable<Expression> dependencies)
        => throw new InvalidOperationException();
    protected override Expression SupportGraph(ComputeGraph graph, IEnumerable<Expression> roots)
        => throw new InvalidOperationException();
}

file record RecursiveTypeTransformer(Variable RecursionLevel) : TypeTransformer()
{
    protected override Expression Other(ComputeUnit unit, IEnumerable<Expression> dependencies)
    {
        return unit switch
        {
            TableColumn.CellReference c => TableCellReference(c, dependencies),
            ComputedChainColumn.CellReference c => ComputedCellReference(c, dependencies),
            RecursiveChainColumn.RecursiveCellReference c => RecursiveCellReference(c, dependencies),
            _ => throw new InvalidOperationException()
        };
    }

    private Expression RecursiveCellReference(RecursiveChainColumn.RecursiveCellReference unit, IEnumerable<Expression> _)
    {
        // Recusive Cell Reference
        Expression expression = new FunctionCall(unit.ColumnName, [new FunctionCall("-", [RecursionLevel, new Constant(unit.Recursion)])]);
        return expression;
    }

    private Expression ComputedCellReference(ComputedChainColumn.CellReference unit, IEnumerable<Expression> _)
    {
        return new Variable(unit.ColumnName);
    }

    private Expression TableCellReference(TableColumn.CellReference unit, IEnumerable<Expression> dependencies)
    {
        return new Variable(unit.ColumnName);
    }
}
