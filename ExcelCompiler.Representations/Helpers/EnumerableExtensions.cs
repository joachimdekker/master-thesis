namespace ExcelCompiler.Representations.Helpers;

public static class EnumerableExtensions
{
    public static IEnumerable<TResult> Select<TKey, TValue, TResult>(this IDictionary<TKey, TValue> dictionary,
        Func<TKey, TValue, int, TResult> selector)
    {
        return dictionary.Select((kv, i) => selector(kv.Key, kv.Value, i));
    }
    
    public static T MaxOrDefault<T>(this IEnumerable<T> enumerable, T defaultValue) where T : IComparable<T>
    {
        return enumerable.DefaultIfEmpty(defaultValue).Max()!;
    }
}