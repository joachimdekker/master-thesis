using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.References;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;
using DefaultComputation = ExcelCompiler.Representations.Compute.Nil;
using Type = ExcelCompiler.Representations.Compute.Type;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class TypeInference
{
    private static readonly Type BooleanType = new Type("boolean");
    
    public static readonly Dictionary<string, Func<List<Type>, Type>> InferenceRules = new()
    {
        {
            "SUM", types =>
            {
                if (types.Any(t => t != types[0]))
                    throw new InvalidOperationException("SUM is not supported for types other than double");

                return types[0];
            }
        },
        {
            "IF", types =>
            {
                // There should be three types
                if (types.Count != 3)
                {
                    throw new InvalidOperationException("IF is not supported for more than three types");
                }

                if (types[0] != BooleanType)
                    throw new InvalidOperationException("IF is not supported for types other than bool");
                if (types[1] != types[2])
                    throw new InvalidOperationException("IF is not supported for types other than the same type");

                return types[1];
            }
        },
        {
            "-", t => CheckOperatorTypes(t, "MINUS")
        },
        {
            "+", t => CheckOperatorTypes(t, "PLUS")
        },
        {
            "*", t => CheckOperatorTypes(t, "TIMES")
        },
        {
            "/", t => CheckOperatorTypes(t, "DIVIDE")
        }
    };

    private static Type CheckOperatorTypes(List<Type> types, string name)
    {
        // There should be two types
        if (types.Count != 2)
        {
            throw new InvalidOperationException($"{name} is not supported for more than two types");
        }

        if (types[0] != types[1]) throw new InvalidOperationException($"{name} is not supported for types other than the same type");

        // Should be a numerical type
        string[] numericalTypes = ["Double", "double", "int", "Int32", "long"];
        if (!numericalTypes.Contains(types[0].Name)) throw new InvalidOperationException($"{name} is not supported for types other than double, int, or long");

        return types[0];
    }

    public ComputeGraph Transform(ComputeGraph graph)
    {
        var transformer = new TypeInferenceTransformer(graph.Constructs);
        var computeGraph = transformer.Transform(graph);
        
        var inferencedConstructs = from construct in computeGraph.Constructs select Transform(construct, transformer);
        
        return computeGraph with
        {
            Constructs = inferencedConstructs.ToList(),
        };
    }

    private Construct Transform(Construct construct, TypeInferenceTransformer transformer)
    {
        return construct switch
        {
            Table table => Transform(table, transformer),
            Chain chain => Transform(chain, transformer),
            _ => throw new InvalidOperationException($"Unsupported construct type: {construct.GetType()}."),
        };
    }
    
    private Table Transform(Table construct, TypeInferenceTransformer transformer)
    {
        List<TableColumn> columns = construct.Columns.Select(c =>
        {
            if (c.ColumnType is TableColumn.TableColumnType.Computed)
            {
                var computation = c.Computation;
                computation = transformer.Transform(computation!);
                return c with
                {
                    Computation = computation,
                    Type = computation.Type,
                };
            }

            return c;
        }).ToList();

        return construct with
        {
            Columns = columns,
        };
    }
    
    private Chain Transform(Chain construct, TypeInferenceTransformer transformer)
    {
        List<ChainColumn> columns = construct.Columns.Select(c =>
        {
            if (c is ComputedChainColumn computedChainColumn)
            {
                var computation = computedChainColumn.Computation;
                computation = transformer.Transform(computation!);
                return computedChainColumn with
                {
                    Computation = computation,
                    Type = computation.Type,
                };
            }

            if (c is RecursiveChainColumn recursiveChainColumn)
            {
                var computation = recursiveChainColumn.Computation;
                computation = transformer.Transform(computation!);
                return recursiveChainColumn with
                {
                    Computation = computation,
                    Type = computation.Type,
                };
            }

            return c;
        }).ToList();

        return construct with
        {
            Columns = columns,
        };
    }
}

public record TypeInferenceTransformer : UnitComputeGraphTransformer
{
    private readonly List<Construct> _constructs;

    public TypeInferenceTransformer(List<Construct> constructs)
    {
        _constructs = constructs;
    }

    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        // Get the types of the dependencies
        var types = dependencies.Where(d => d is not DefaultComputation).Select(d => d.Type).Distinct();

        // Get the type of the range based on the types
        Type type = types.SingleOrDefault()
                    ?? throw new InvalidOperationException("Range references with multiple types are not supported.");

        return rangeReference with
        {
            Type = type,
            Dependencies = dependencies.ToList()
        };
    }

    protected override ComputeUnit Function(Function function, IEnumerable<ComputeUnit> dependencies)
    {
        // Get the types of the dependencies
        List<Type> types = dependencies.Select(d => d.Type).ToList();

        // Get the type of the function based on the name and the dependencies
        if (!TypeInference.InferenceRules.TryGetValue(function.Name, out var inferenceRule))
            throw new InvalidOperationException($"Unknown function {function.Name}");

        Type type = inferenceRule(types);

        // Create the new function
        return function with
        {
            Type = type,
            Dependencies = dependencies.ToList()
        };
    }

    protected override ComputeUnit CellReference(CellReference cellReference, IEnumerable<ComputeUnit> dependencies)
    {
        // The cell reference should have one dependency, take that type.
        var dependency = dependencies.SingleOrDefault() ?? throw new InvalidOperationException("Cell reference should have one dependency.");
        var type = dependency.Type;

        // Create the new cell reference
        return cellReference with
        {
            Dependencies = dependencies.ToList(),
            Type = type
        };
    }

    protected override ComputeUnit TableReference(TableReference tableReference, IEnumerable<ComputeUnit> _)
    {
        var type = _constructs.OfType<Representations.Compute.Specialized.Table>()
            .Single(t => t.Name == tableReference.Reference.TableName)
            .Columns
            .Single(c => c.Name == tableReference.Reference.ColumnNames[0])
            .Type;

        // Create the new table reference
        return tableReference with
        {
            Type = type,
        };
    }

    protected override ComputeUnit Nil(Nil nil, IEnumerable<ComputeUnit> _)
    {
        return nil with
        {
            Type = new Type(typeof(double)), // Set the type to double for now.
        };
    }

    protected override ComputeUnit Input(Input input, IEnumerable<ComputeUnit> _)
    {
        return input with { Type = input.Type ?? new Type(typeof(double)) };
    }

    private ComputeUnit UpdateCellReferenceWithTypeAndDependencies(
    ComputeUnit reference, 
    IEnumerable<ComputeUnit> dependencies)
{
    var (type, column) = GetColumnTypeForLocation(reference.Location);
    
    // Better return?
    // return reference with
    // {
    //     Dependencies = dependencies.ToList(),
    //     Type = type,
    // };
    
    return reference switch
    {
        ComputedChainColumn.CellReference cr => cr with { Dependencies = dependencies.ToList(), Type = type },
        RecursiveChainColumn.RecursiveCellReference rcr => rcr with { Dependencies = dependencies.ToList(), Type = type },
        DataChainColumn.Reference dcr => dcr with { Dependencies = dependencies.ToList(), Type = type },
        _ => throw new InvalidOperationException($"Unsupported cell reference: {reference.GetType()}."),
    };
}

private (Type Type, object Column) GetColumnTypeForLocation(Location location)
{
    var chain = _constructs.OfType<Chain>().Single(c => c.Location.Contains(location));
    var column = chain.Columns.Single(c => c.Location.Contains(location));
    return (column.Type, column);
}

protected override ComputeUnit Other(ComputeUnit unit, IEnumerable<ComputeUnit> dependencies)
{
    return unit switch
    {
        DataChainColumn.Reference 
            or ComputedChainColumn.CellReference 
            or RecursiveChainColumn.RecursiveCellReference =>
            UpdateCellReferenceWithTypeAndDependencies(unit, dependencies),
        TableColumn.CellReference tableCellReference => TableCellReference(tableCellReference, dependencies),
        _ => throw new InvalidOperationException($"Unsupported cell type: {unit.GetType()}."),
    };
}

private ComputeUnit TableCellReference(TableColumn.CellReference tableCellReference, IEnumerable<ComputeUnit> dependencies)
{
    var table = _constructs.OfType<Table>().Single(t => t.Location.Contains(tableCellReference.Location));
    var column = table.Columns.Single(c => c.Location.Contains(tableCellReference.Location));

    return tableCellReference with
    {
        Dependencies = dependencies.ToList(),
        Type = column.Type
    };
}
}
