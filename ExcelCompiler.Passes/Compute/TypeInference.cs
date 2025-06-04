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
        return new TypeInferenceTransformer(graph.Tables).Transform(graph);
    }
}

public record TypeInferenceTransformer : UnitSupportGraphTransformer
{
    private readonly List<Table> _tables;

    public TypeInferenceTransformer(List<Table> tables)
    {
        _tables = tables;
    }

    protected override ComputeUnit Constant(Location location, IEnumerable<ComputeUnit> dependencies, Type _, object value)
    {
        Type type = value.GetType();
        return base.Constant(location, dependencies, type, value);
    }

    protected override ComputeUnit RangeReference(Location location, IEnumerable<ComputeUnit> dependencies, Range reference)
    {
        // Get the types of the dependencies
        List<Type> types = dependencies.Select(d => d.Type).Distinct().ToList();

        // Get the type of the range based on the types
        Type type = types.Count switch
        {
            1 => types[0],
            _ => throw new InvalidOperationException("Range references with multiple types are not supported.")
        };

        // Create the new range reference
        RangeReference rangeReference = new RangeReference(location, reference)
        {
            Type = type,
        };

        rangeReference.AddDependencies(dependencies);

        return rangeReference;
    }

    protected override ComputeUnit Function(Location location, IEnumerable<ComputeUnit> dependencies, string name)
    {
        // Get the types of the dependencies
        List<Type> types = dependencies.Select(d => d.Type).ToList();

        // Get the type of the function based on the name and the dependencies
        if (!TypeInference.InferenceRules.TryGetValue(name, out var inferenceRule)) 
            throw new InvalidOperationException($"Unknown function {name}");
    
        Type type = inferenceRule(types);

        // Create the new function
        Function function = new Function(location, name)
        {
            Type = type,
        };

        function.AddDependencies(dependencies);

        return function;
    }

    protected override ComputeUnit CellReference(Location location, IEnumerable<ComputeUnit> dependencies, Location reference)
    {
        // The cell reference should have one dependency, take that type.
        if (dependencies.Count() != 1) throw new InvalidOperationException("Cell reference should have one dependency.");

        var dependency = dependencies.Single();
        var type = dependency.Type;

        // Create the new cell reference
        CellReference cellReference = new CellReference(location, reference)
        {
            Type = type
        };

        cellReference.AddDependency(dependency);

        return cellReference;
    }

    protected override ComputeUnit TableReference(Location location, IEnumerable<ComputeUnit> dependencies, Representations.References.TableReference reference)
    {
        var type = _tables.Single(t => t.Name == reference.TableName).Columns.Single(c => c.Name == reference.ColumnNames[0]).Type;

        // Create the new table reference
        TableReference tableReference = new(location, reference)
        {
            Type = type,
        };

        return tableReference;
    }

    protected override ComputeUnit Nil(Location location, IEnumerable<ComputeUnit> dependencies)
    {
        return new Nil(location)
        {
            Type = typeof(double), // Set the type to double for now.
        };
    }
}
