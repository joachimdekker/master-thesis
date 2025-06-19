using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.Data.Preview;
using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Passes.Preview.Code;

public class GenerateTypes
{
    public List<Class> Generate(SupportGraph graph, DataManager dataManager)
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
        var computedProperties = chain.Columns.OfType<ComputedChainColumn>().Select(c => new Property(c.Name, new Type(c.Type))
        {
            Getter = typeTransformer.Transform(c.Computation!),
        }).ToList();
        var chainProperties

        return new(chain.Name, [..properties, ..computedProperties], methods);
    }

    public Class Generate(Table table)
    {

    }
}

file record TypeTransformer() : SupportGraphTransformer<Expression, Expression>
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
    protected override Expression SupportGraph(SupportGraph graph, IEnumerable<Expression> roots)
        => throw new InvalidOperationException();
}
