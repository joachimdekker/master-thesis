using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.References;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class InsertInputs
{
    public ComputeGrid Transform(ComputeGrid grid, ICollection<Location> inputs)
    {
        foreach (var input in inputs)
        {
            grid[input] = new Input(grid[input].Type!, input);
        }
        
        return grid;
    }
}