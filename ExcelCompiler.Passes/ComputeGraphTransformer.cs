using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;

namespace ExcelCompiler.Passes;

public abstract record ComputeGraphTransformer<TRes, TVal>
{
    protected Dictionary<ComputeUnit, TVal> _valueCache { get; init; } = new();
    
    protected abstract TVal CellReference(CellReference cellReference, IEnumerable<TVal> dependencies);

    protected abstract TVal RangeReference(RangeReference rangeReference, IEnumerable<TVal> dependencies);

    protected abstract TVal TableReference(TableReference tableReference, IEnumerable<TVal> dependencies);

    protected abstract TVal DataReference(DataReference dataReference, IEnumerable<TVal> dependencies);

    protected abstract TVal Function(Function function, IEnumerable<TVal> dependencies);

    protected abstract TVal Nil(Nil nil, IEnumerable<TVal> dependencies);

    protected abstract TVal Constant<T>(ConstantValue<T> constant, IEnumerable<TVal> dependencies);
    
    protected abstract TVal Input(Input input, IEnumerable<TVal> dependencies);

    protected abstract TRes SupportGraph(ComputeGraph graph, IEnumerable<TVal> roots);

    protected virtual TVal Other(ComputeUnit unit, IEnumerable<TVal> dependencies) => throw new ArgumentException($"Unsupported cell type: {unit.GetType()}.", nameof(unit));
    
    public virtual TRes Transform(ComputeGraph graph)
    {
        IEnumerable<TVal> roots = graph.Roots.Select(Transform);
        return SupportGraph(graph, roots);
    }

    public virtual TVal Transform(ComputeUnit unit)
    {
        if (_valueCache.TryGetValue(unit, out var cached))
        {
            return cached;
        }

        var dependencies = unit.Dependencies.Select(d => Transform(d));
        TVal value = unit switch
        {
            CellReference cellReference => CellReference(cellReference, dependencies),
            RangeReference rangeReference => RangeReference(rangeReference, dependencies),
            TableReference tableReference => TableReference(tableReference, dependencies),
            DataReference dataReference => DataReference(dataReference, dependencies),
            Function function => Function(function, dependencies),
            Nil nil => Nil(nil, dependencies),
            Input input => Input(input, dependencies),
            // ConstantValue<object> constant => Constant(location, dependencies, constant.Type, constant.Value),
            ConstantValue<string> constant => Constant(constant, dependencies),
            ConstantValue<double> constant => Constant(constant, dependencies),
            ConstantValue<bool> constant => Constant(constant, dependencies),
            ConstantValue<DateTime> constant => Constant(constant, dependencies),
            _ => Other(unit, dependencies),
        };

        _valueCache[unit] = value;
        return value;
    }
}

/// <summary>
/// Support Graph Transformer with default values
/// </summary>
/// <remarks>
/// Make them immutable i.e., we make new instances everytime.
/// </remarks>
public abstract record UnitComputeGraphTransformer : ComputeGraphTransformer<ComputeGraph, ComputeUnit>
{
    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        return cellReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        return rangeReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit TableReference(TableReference tableReference, IEnumerable<ComputeUnit> dependencies)
    {
        return tableReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit DataReference(DataReference dataReference, IEnumerable<ComputeUnit> dependencies)
    {
        return dataReference with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit Function(Function function, IEnumerable<ComputeUnit> dependencies)
    {
        return function with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit Nil(Nil nil, IEnumerable<ComputeUnit> dependencies)
    {
        return nil with { Dependencies = dependencies.ToList() };
    }

    protected override ComputeUnit Constant<T>(ConstantValue<T> constant, IEnumerable<ComputeUnit> dependencies)
    {
        return constant with { Dependencies = dependencies.ToList() };
    }
    
    protected override ComputeUnit Input(Input input, IEnumerable<ComputeUnit> dependencies) => input with {Dependencies = dependencies.ToList()};

    protected override ComputeGraph SupportGraph(ComputeGraph graph, IEnumerable<ComputeUnit> roots)
    {
        return graph with { Roots = roots.ToList() };
    }
    
    protected override ComputeUnit Other(ComputeUnit unit, IEnumerable<ComputeUnit> dependencies) => unit with {Dependencies = dependencies.ToList()};
}
