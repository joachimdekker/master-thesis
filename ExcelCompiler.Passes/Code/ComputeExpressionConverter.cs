using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.Compute;
using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Passes.Code;

[CompilerPass]
public class ComputeExpressionConverter
{
    public Expression Transform(ComputeUnit computeUnit)
    {
        // TODO: Type inference (Probably in the Compute Layer
        Type type = new Type("double");

        return computeUnit switch
        {
            ConstantValue<double> @double => new Constant(type, @double.Value),
            Function func => new FunctionCall(func.Name, func.Dependencies.Select(Transform).ToList()),
            CellReference cell => Transform(cell.Dependencies[0]),
            RangeReference range => new ListExpression(range.Dependencies.Select(Transform).ToList()),

            _ => throw new InvalidOperationException($"Unsupported compute unit {computeUnit.GetType().Name}")
        };
    }
}