namespace ExcelCompiler.Representations.Helpers;

public static class TypeExtensions
{
    public static object GetDefaultValue(this Type t)
    {
        if (!t.IsValueType || Nullable.GetUnderlyingType(t) != null) return null!;
        
        return Activator.CreateInstance(t)!;
    }

    public static CodeLayout.Type Convert(this Compute.Type? type) => type is null ? new(typeof(double)) : new(type.Name);
}