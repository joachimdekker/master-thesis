namespace ExcelCompiler.Evaluation.Utils;

public static class StringHelpers
{
    public static string Combine(this IEnumerable<string> strings, string combinator = " ") => string.Join(combinator, strings);
}