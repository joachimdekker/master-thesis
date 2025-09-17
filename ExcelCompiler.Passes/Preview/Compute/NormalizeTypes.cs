using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class NormalizeTypes
{
    private readonly static Dictionary<string, string> typeMappings = new()
    {
        { "int", "double" },
        { "Int32", "double" },
        { "Int64", "double" },
        { "double", "double" },
        { "Double", "double" },
        { "float", "double" },
        { "decimal", "double" },
        { "long", "double" },
        { "short", "double" },
        { "byte", "double" },
        { "uint", "double" },
        { "ulong", "double" },
        { "ushort", "double" },
        { "sbyte", "double" },
        { "char", "string" },
        { "bool", "string" }, // Excel represents booleans as strings
    };

    public ComputeGraph Transform(ComputeGraph graph)
    {
        var roots = graph.Roots.Select(Transform).ToList();

        return graph with { Roots = roots };
    }

    public ComputeUnit Transform(ComputeUnit unit)
    {
        var deps = unit.Dependencies.Select(Transform);

        if (unit is not { Type: { Name: string type } }) 
            return unit with { Dependencies = deps.ToList() };

        if (!typeMappings.TryGetValue(type, out string normalizedTypeName)) throw new InvalidOperationException($"Unsupported type {type}");

        return unit with
        {
            Type = unit.Type with { Name = normalizedTypeName },
            Dependencies = deps.ToList()
        };
    }
}


