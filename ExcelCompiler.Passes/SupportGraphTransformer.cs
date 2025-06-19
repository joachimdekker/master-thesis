using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.References.Range;
using Table = ExcelCompiler.Representations.Compute.Specialized.Table;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;

namespace ExcelCompiler.Passes;

public abstract record SupportGraphTransformer<TRes, TVal>
{
    protected abstract TVal CellReference(CellReference cellReference, IEnumerable<TVal> dependencies);

    protected abstract TVal RangeReference(RangeReference rangeReference, IEnumerable<TVal> dependencies);

    protected abstract TVal TableReference(TableReference tableReference, IEnumerable<TVal> dependencies);

    protected abstract TVal DataReference(DataReference dataReference, IEnumerable<TVal> dependencies);

    protected abstract TVal Function(Function function, IEnumerable<TVal> dependencies);

    protected abstract TVal Nil(Nil nil, IEnumerable<TVal> dependencies);

    protected abstract TVal Constant<T>(ConstantValue<T> constant, IEnumerable<TVal> dependencies);

    protected abstract TRes SupportGraph(SupportGraph graph, IEnumerable<TVal> roots);

    public virtual TRes Transform(SupportGraph graph)
    {
        Dictionary<ComputeUnit, TVal> valueCache = new();
        IEnumerable<TVal> roots = graph.Roots.Select(r => Transform(r, valueCache));
        return SupportGraph(graph, roots);
    }

    protected TVal Transform(ComputeUnit unit, Dictionary<ComputeUnit, TVal> valueCache)
    {
        if (valueCache.TryGetValue(unit, out var cached))
        {
            return cached;
        }

        var dependencies = unit.Dependencies.Select(d => Transform(d, valueCache));
        TVal value = unit switch
        {
            CellReference cellReference => CellReference(cellReference, dependencies),
            RangeReference rangeReference => RangeReference(rangeReference, dependencies),
            TableReference tableReference => TableReference(tableReference, dependencies),
            DataReference dataReference => DataReference(dataReference, dependencies),
            Function function => Function(function, dependencies),
            Nil nil => Nil(nil, dependencies),
            // ConstantValue<object> constant => Constant(location, dependencies, constant.Type, constant.Value),
            ConstantValue<string> constant => Constant(constant, dependencies),
            ConstantValue<double> constant => Constant(constant, dependencies),
            ConstantValue<bool> constant => Constant(constant, dependencies),
            ConstantValue<DateTime> constant => Constant(constant, dependencies),
            _ => throw new ArgumentException("Unsupported cell type.", nameof(unit))
        };

        valueCache[unit] = value;
        return value;
    }

    public virtual TVal Transform(ComputeUnit unit)
    {
        Dictionary<ComputeUnit, TVal> valueCache = new();
        return Transform(unit, valueCache);
    }
}

/// <summary>
/// Support Graph Transformer with default values
/// </summary>
/// <remarks>
/// Make them immutable i.e., we make new instances everytime.
/// </remarks>
public abstract record UnitSupportGraphTransformer : SupportGraphTransformer<SupportGraph, ComputeUnit>
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

    protected override SupportGraph SupportGraph(SupportGraph graph, IEnumerable<ComputeUnit> roots)
    {
        return graph with { Roots = roots.ToList() };
    }
}
