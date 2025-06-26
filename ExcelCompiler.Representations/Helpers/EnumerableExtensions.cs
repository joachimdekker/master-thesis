namespace ExcelCompiler.Representations.Helpers;

public static class EnumerableExtensions
{
    public static IEnumerable<TResult> Select<TKey, TValue, TResult>(this IDictionary<TKey, TValue> dictionary,
        Func<TKey, TValue, int, TResult> selector)
    {
        return dictionary.Select((kv, i) => selector(kv.Key, kv.Value, i));
    }
}