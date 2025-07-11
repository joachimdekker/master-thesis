using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.Data.Preview;
using ExcelCompiler.Representations.Helpers;
using ListOf = ExcelCompiler.Representations.CodeLayout.ListOf;
using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Passes.Preview.Code;

[CompilerPass]
public class GenerateTypes
{
    public List<(Construct Construct, Class Type)> Generate(ComputeGraph graph, DataManager dataManager)
    {
        IEnumerable<(Construct Construct, Class Type)>? types =
            from construct in graph.Constructs
            let type = construct switch
            {
                Table table => Generate(table),
                Chain chain => Generate(chain),
                _ => throw new InvalidOperationException()
            }
            select (construct, type);

        return types.ToList();
    }

    public Class Generate(Chain chain)
    {
        var typeTransformer = new TypeTransformer();

        var properties = chain.Columns.OfType<DataChainColumn>().Select(c => new Property(c.Name, new ListOf(c.Type.Convert()))).ToList();
        var computedProperties = chain.Columns
            .OfType<ComputedChainColumn>()
            .Select(c =>
            {
                Variable counter = new Variable("counter", new Type(typeof(int)));
                RecursiveTypeTransformer transformer = new(counter);
                Statement[] body = [new Return(transformer.Transform(c.Computation!))];

                return new Method(c.Name + "At", [counter], body);
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

        return new(chain.Name, [..properties], [..chainProperties,..computedProperties]);
    }

    private Statement[] GenerateRecursiveBody(RecursiveChainColumn recursiveChainColumn, Variable counter)
    {
        RecursiveTypeTransformer transformer = new(counter);
        List<Statement> body = new();

        if (recursiveChainColumn.Initialization is not null)
        {
            Statement baseCase = new If(new FunctionCall("Equals", [counter, new Constant(new Type(typeof(int)), 0)]), [new Return(transformer.Transform(recursiveChainColumn.Initialization!))]);
            body.Add(baseCase);
        }
        

        Statement recursiveCase = new Return(transformer.Transform(recursiveChainColumn.Computation!));
        
        
        return [..body, recursiveCase];
    }

    public Class Generate(Table table)
    {
        var typeTransformer = new TypeTransformer();

        var properties = table.Columns.Where(tc => tc.Computation is null).Select(tc => new Property(tc.Name, tc.Type.Convert())).ToList();
        var computedProperties = table.Columns.Where(tc => tc.Computation is not null).Select(tc => new Property(tc.Name, tc.Type.Convert())
        {
            Getter = typeTransformer.Transform(tc.Computation!),
        }).ToList();

        return new(table.Name + "Item", [..properties, ..computedProperties], []);
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

    protected override Expression Input(Input input, IEnumerable<Expression> dependencies)
    {
        throw new InvalidOperationException();
    }

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
        Expression expression = new FunctionCall(unit.ColumnName + "At", [new FunctionCall("-", [RecursionLevel, new Constant(unit.Recursion)])]);
        return expression;
    }

    private Expression ComputedCellReference(ComputedChainColumn.CellReference unit, IEnumerable<Expression> _)
    {
        return new ListAccessor(unit.Type.Convert(), new Variable(unit.ColumnName), new FunctionCall("-", [RecursionLevel, new Constant(unit.Index + 1)]));
    }

    private Expression TableCellReference(TableColumn.CellReference unit, IEnumerable<Expression> _)
    {
        return new Variable(unit.ColumnName);
    }
}
