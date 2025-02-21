namespace ExcelCompiler.Domain.Compute;

public class ConstantValue<TValue>(TValue value) : Function("constant" + nameof(TValue), new List<Function>())
{
    TValue Value { get; } = value;
    
    Type Type { get; } = typeof(TValue);
}