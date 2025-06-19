using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.References.Range;
using Table = ExcelCompiler.Representations.Compute.Specialized.Table;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;

namespace ExcelCompiler.Passes.Compute;

[CompilerPass]
public class TypeInference
{
    public static readonly Dictionary<string, Func<List<Type>, Type>> InferenceRules = new()
    {
        {
            "SUM", types =>
            {
                if (!types.All(t => t == types[0]))
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

                if (types[0] != typeof(bool))
                    throw new InvalidOperationException("IF is not supported for types other than bool");
                if (types[1] != types[2])
                    throw new InvalidOperationException("IF is not supported for types other than the same type");

                return types[1];
            }
        },
        {
            "-", types =>
            {
                // There should be two types
                if (types.Count != 2)
                {
                    throw new InvalidOperationException("MINUS is not supported for more than two types");
                }

                if (types[0] != types[1])
                    throw new InvalidOperationException("MINUS is not supported for types other than the same type");

                // Should be a numerical type
                if (types[0] != typeof(double) && types[0] != typeof(int) && types[0] != typeof(long))
                    throw new InvalidOperationException("MINUS is not supported for types other than double, int, or long");

                return types[0];
            }
        },
        {
            "+", types =>
            {
                // There should be two types
                if (types.Count != 2)
                {
                    throw new InvalidOperationException("MINUS is not supported for more than two types");
                }

                if (types[0] != types[1])
                    throw new InvalidOperationException("MINUS is not supported for types other than the same type");

                // Should be a numerical type
                if (types[0] != typeof(double) && types[0] != typeof(int) && types[0] != typeof(long))
                    throw new InvalidOperationException("MINUS is not supported for types other than double, int, or long");

                return types[0];
            }
        },
        {
            "*", types =>
            {
                // There should be two types
                if (types.Count != 2)
                {
                    throw new InvalidOperationException("MINUS is not supported for more than two types");
                }

                if (types[0] != types[1])
                    throw new InvalidOperationException("MINUS is not supported for types other than the same type");

                // Should be a numerical type
                if (types[0] != typeof(double) && types[0] != typeof(int) && types[0] != typeof(long))
                    throw new InvalidOperationException("MINUS is not supported for types other than double, int, or long");

                return types[0];
            }
        },
        {
            "/", types =>
            {
                // There should be two types
                if (types.Count != 2)
                {
                    throw new InvalidOperationException("MINUS is not supported for more than two types");
                }

                if (types[0] != types[1])
                    throw new InvalidOperationException("MINUS is not supported for types other than the same type");

                // Should be a numerical type
                if (types[0] != typeof(double) && types[0] != typeof(int) && types[0] != typeof(long))
                    throw new InvalidOperationException("MINUS is not supported for types other than double, int, or long");

                return types[0];
            }
        }
    };

    public SupportGraph Transform(SupportGraph graph)
    {
        return new TypeInferenceTransformer(graph.Constructs).Transform(graph);
    }
}

public record TypeInferenceTransformer : UnitSupportGraphTransformer
{
    private readonly List<Table> _tables;

    public TypeInferenceTransformer(List<Table> tables)
    {
        _tables = tables;
    }

    protected override ComputeUnit RangeReference(RangeReference rangeReference, IEnumerable<ComputeUnit> dependencies)
    {
        // Get the types of the dependencies
        List<Type> types = dependencies.Select(d => d.Type).Distinct().ToList();

        // Get the type of the range based on the types
        Type type = dependencies.Select(d => d.Type).Distinct().SingleOrDefault()
                    ?? throw new InvalidOperationException("Range references with multiple types are not supported.");

        rangeReference.AddDependencies(dependencies);

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
        var type = _tables
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
            Type = typeof(double), // Set the type to double for now.
        };
    }
}
