using Location = ExcelCompiler.Representations.References.Location;

namespace ExcelCompiler.Representations.Compute;

public record ConstantValue<TValue> : ComputeUnit
{
    public ConstantValue(TValue value, Location location) : base(location)
    {
        Type = typeof(TValue);
        Value = value;
    }

    public TValue Value { get; }
}