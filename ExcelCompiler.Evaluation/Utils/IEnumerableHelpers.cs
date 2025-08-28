using System.Runtime.CompilerServices;

namespace ExcelCompiler.Evaluation.Utils;

public static class IEnumerableHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Random<T>(this ICollection<T> enumerable) => enumerable.Random(System.Random.Shared);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Random<T>(this ICollection<T> collection, Random rng) => collection.ElementAt(rng.Next(collection.Count));
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Random<T>(this IEnumerable<T> enumerable) => enumerable.Random(System.Random.Shared);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Random<T>(this IEnumerable<T> enumerable, Random rng) => enumerable.ElementAt(rng.Next(enumerable.Count()));
}