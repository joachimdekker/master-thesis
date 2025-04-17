using Location = ExcelCompiler.Domain.Structure.Location;

namespace ExcelCompiler.Domain.Compute;

public class ConstantValue<TValue>(TValue value, Location location) : ComputeUnit(location)
{
    public TValue Value { get; } = value;
    
    public Type Type { get; } = typeof(TValue);
    public override bool IsConstant => true;
}