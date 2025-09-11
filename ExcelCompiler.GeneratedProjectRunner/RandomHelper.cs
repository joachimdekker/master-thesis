namespace ExcelCompiler.GeneratedProjectRunner;

public static class RandomHelper
{
    public static double NextDouble(this Random random, double min, double max)
        => random.NextDouble() * (max - min) + min;
}