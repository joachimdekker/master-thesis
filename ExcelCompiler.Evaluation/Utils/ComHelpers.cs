using static System.Runtime.InteropServices.Marshal;

namespace ExcelCompiler.Evaluation.Utils;

public static class ComHelpers
{
    public static void Release(object? comObj)
    {
        if (comObj == null) return;
        try { FinalReleaseComObject(comObj); }
        catch { /* ignore */ }
    }
}