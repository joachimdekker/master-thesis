using Location = ExcelCompiler.Representations.Structure.Location;

namespace ExcelCompiler.Representations.Compute;

public class ConstantValue<TValue> : ComputeUnit
{
    public ConstantValue(TValue value, Location location) : base(location)
    {
        Type = typeof(TValue);
        Value = value;
    }

    public TValue Value { get; }
    
    public override bool IsConstant => true;
}