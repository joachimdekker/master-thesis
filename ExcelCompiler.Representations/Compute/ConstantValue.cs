using Location = ExcelCompiler.Representations.Structure.Location;

namespace ExcelCompiler.Representations.Compute;

public class ConstantValue<TValue>(TValue value, Location location) : ComputeUnit(location)
{
    public TValue Value { get; } = value;
    
    public Type Type { get; } = typeof(TValue);
    public override bool IsConstant => true;
}