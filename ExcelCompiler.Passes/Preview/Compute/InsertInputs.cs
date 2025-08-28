using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.References;
using Microsoft.Extensions.Logging;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class InsertInputs(ILogger<InsertInputs> logger)
{
    public ComputeGrid Transform(ComputeGrid grid, ICollection<Location> inputs)
    {
        foreach (var input in inputs)
        {
            if (!grid.TryGetValue(input, out var original))
            {
                logger.LogWarning("Input {input} could not be inserted into the code. " +
                                  "This is probably because the input is not used in the calculation of the output.", 
                    input.ToA1());
                continue;
            };
            grid[input] = new Input(original.Type!, input);
        }
        
        return grid;
    }
}