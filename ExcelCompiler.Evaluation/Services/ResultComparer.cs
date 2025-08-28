using System.Globalization;
using ExcelCompiler.Evaluation.Models;

namespace ExcelCompiler.Evaluation.Services;

public sealed class ResultComparer
{
    public bool Compare(
        IDictionary<CellReference, object> expected,
        IDictionary<CellReference, object> actual,
        double tolerance,
        out List<string> diffs)
    {
        diffs = new List<string>();

        // Missing or mismatched
        foreach (var (key, exp) in expected)
        {
            if (!actual.TryGetValue(key, out var act))
            {
                diffs.Add($"Missing output: {key} (expected={Fmt(exp)})");
                continue;
            }

            if (!ValuesEqual(exp, act, tolerance))
                diffs.Add($"Mismatch {key}: expected={Fmt(exp)} actual={Fmt(act)}");
        }

        // Extra outputs
        foreach (var key in actual.Keys)
        {
            if (!expected.ContainsKey(key))
                diffs.Add($"Unexpected output provided: {key}={Fmt(actual[key])}");
        }

        return diffs.Count == 0;
    }

    private static bool ValuesEqual(object a, object b, double t)
    {
        if (a is double da && b is double db) return Math.Abs(da - db) <= t;

        if (TryParseDouble(a, out var da2) && TryParseDouble(b, out var db2))
            return Math.Abs(da2 - db2) <= t;

        return string.Equals(Convert.ToString(a), Convert.ToString(b), StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseDouble(object o, out double d) =>
        double.TryParse(Convert.ToString(o, CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out d);

    private static string Fmt(object o) =>
        o is double d ? d.ToString("G17", CultureInfo.InvariantCulture) : o?.ToString() ?? "";
}