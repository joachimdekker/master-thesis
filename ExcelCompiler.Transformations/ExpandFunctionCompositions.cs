using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Transformations;

public class ExpandFunctionCompositions
{
    public List<ComputeUnit> Transform(List<ComputeUnit> units)
    {
        List<ComputeUnit> results = new List<ComputeUnit>();
        foreach (var unit in units)
        {
            if (unit is  not FunctionComposition functionComposition)
            {
                results.Add(unit);
                continue;
            }
            
            Function func = Transform(functionComposition);
            results.Add(func);
        }
        
        return results;
    }

    public Function Transform(FunctionComposition unit)
    {
        Function function = new Function(unit.Name, unit.Location);

        foreach (var argument in unit.Arguments)
        {
            if (argument is not FunctionComposition functionComposition)
            {
                function.AddDependency(argument);
                continue;
            }
            
            Function func = Transform(functionComposition);
            function.AddDependency(func);
        }
        
        return function;
    }
}