using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.Structure.Range;
using Table = ExcelCompiler.Representations.Compute.Specialized.Table;
using TableReference = ExcelCompiler.Representations.Structure.TableReference;

namespace ExcelCompiler.Passes;

public abstract record SupportGraphTransformer<TRes, TVal>
{
    protected abstract TVal CellReference(Location location, IEnumerable<TVal> dependencies, Location reference);

    protected abstract TVal RangeReference(Location location, IEnumerable<TVal> dependencies, Range reference);

    protected abstract TVal TableReference(Location location, IEnumerable<TVal> dependencies, TableReference reference);

    protected abstract TVal DataReference(Location location, IEnumerable<TVal> dependencies, string repository, string name);

    protected abstract TVal Function(Location location, IEnumerable<TVal> dependencies, string name);

    protected abstract TVal Nil(Location location, IEnumerable<TVal> dependencies);

    protected abstract TVal Constant(Location location, IEnumerable<TVal> dependencies, Type type, object value);

    protected abstract TRes SupportGraph(IEnumerable<TVal> roots, List<Table> tables);

    public virtual TRes Transform(SupportGraph graph)
    {
        Dictionary<ComputeUnit, TVal> valueCache = new();
        IEnumerable<TVal> roots = graph.Roots.Select(r => Transform(r, valueCache));
        return SupportGraph(roots, graph.Tables);
    }

    protected TVal Transform(ComputeUnit unit, Dictionary<ComputeUnit, TVal> valueCache)
    {
        if (valueCache.TryGetValue(unit, out var cached))
        {
            return cached;
        }

        var location = unit.Location;
        var dependencies = unit.Dependencies.Select(d => Transform(d, valueCache));
        TVal value = unit switch
        {
            CellReference cellReference => CellReference(location, dependencies, cellReference.Reference),
            RangeReference rangeReference => RangeReference(location, dependencies, rangeReference.Reference),
            Representations.Compute.TableReference tableReference => TableReference(location, dependencies, tableReference.Reference),
            DataReference dataReference => DataReference(location, dependencies, dataReference.RepositoryName, dataReference.DataName),
            Function function => Function(location, dependencies, function.Name),
            Nil _ => Nil(location, dependencies),
            // ConstantValue<object> constant => Constant(location, dependencies, constant.Type, constant.Value),
            ConstantValue<string> constant => Constant(location, dependencies, constant.Type, constant.Value),
            ConstantValue<double> constant => Constant(location, dependencies, constant.Type, constant.Value),
            ConstantValue<bool> constant => Constant(location, dependencies, constant.Type, constant.Value),
            ConstantValue<DateTime> constant => Constant(location, dependencies, constant.Type, constant.Value),
            _ => throw new ArgumentException("Unsupported cell type.", nameof(unit))
        };

        valueCache[unit] = value;
        return value;
    }

    public virtual TVal Transform(ComputeUnit unit)
    {
        Dictionary<ComputeUnit, TVal> valueCache = new Dictionary<ComputeUnit, TVal>();
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
    protected override ComputeUnit CellReference(Location location, IEnumerable<ComputeUnit> dependencies, Location reference)
    {
        var unit = new CellReference(location, reference);
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override ComputeUnit RangeReference(Location location, IEnumerable<ComputeUnit> dependencies, Range reference)
    {
        var unit = new RangeReference(location, reference);
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override ComputeUnit TableReference(Location location, IEnumerable<ComputeUnit> dependencies, TableReference reference)
    {
        var unit = new Representations.Compute.TableReference(location, reference);
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override ComputeUnit DataReference(Location location, IEnumerable<ComputeUnit> dependencies, string repository, string name)
    {
        var unit = new DataReference(location)
        {
            RepositoryName = repository,
            DataName = name,
        };
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override ComputeUnit Function(Location location, IEnumerable<ComputeUnit> dependencies, string name)
    {
        var unit = new Function(location, name);
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override ComputeUnit Nil(Location location, IEnumerable<ComputeUnit> dependencies)
    {
        var unit = new Nil(location);
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override ComputeUnit Constant(Location location, IEnumerable<ComputeUnit> dependencies, Type type, object value)
    {
        var method = typeof(ConstantValue<>).MakeGenericType(type);
        var unit = (ComputeUnit)Activator.CreateInstance(method, value, location)!;
        unit.Dependencies.AddRange(dependencies);
        return unit;
    }

    protected override SupportGraph SupportGraph(IEnumerable<ComputeUnit> roots, List<Table> tables)
    {
        return new SupportGraph(roots.ToList())
        {
            Tables = tables,
        };
    }
}
