namespace ExcelCompiler.Representations.Helpers;

public static class TypeExtensions
{
    public static object GetDefaultValue(this Type t)
    {
        if (!t.IsValueType || Nullable.GetUnderlyingType(t) != null) return null!;
        
        return Activator.CreateInstance(t)!;
    }
}