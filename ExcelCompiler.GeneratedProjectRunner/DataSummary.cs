using System.Diagnostics;

public record DataSummary
{
    // In Ticks
    public long Total { get; }
    public double Mean { get; }
    public double Std { get; }

    public double StdErr { get; }
    public (double Left, double Right) Conf95 { get; }

    public double Quartile25 { get; }
    public double Quartile50 { get; }
    public double Quartile75 { get; }

    public double IQR => Quartile75 - Quartile25;

    public double Min { get; }
    public double Max { get; }

    public List<long> Outliers { get; private set; } = [];

    public DataSummary(List<long> data)
    {
        
        List<long> sorted = data.Order().ToList();
        Quartile25 = sorted[(int)(0.25 * (sorted.Count - 1))];
        Quartile50 = sorted[(int)(0.50 * (sorted.Count - 1))];
        Quartile75 = sorted[(int)(0.75 * (sorted.Count - 1))];
        Min = sorted[0];
        Max = sorted[^1];

        data = RemoveOutliers(data);

        Total = data.Sum();
        Mean = CalcMean(data);
        Std = CalcStd(data);
        StdErr = Std / Math.Sqrt(data.Count);
        Conf95 = CalcConf95(data);
    }

    private List<long> RemoveOutliers(List<long> times)
    {
        if (times.Count < 4) return times;
        double q1 = CalcMean(times.Where(t => t <= Quartile25).ToList());
        double q3 = CalcMean(times.Where(t => t >= Quartile75).ToList());
        double iqr = q3 - q1;
        double lowerBound = q1 - 1.5 * iqr;
        double upperBound = q3 + 1.5 * iqr;
        Outliers = times.Where(t => t < lowerBound || t > upperBound).ToList();
        return times.Where(t => t >= lowerBound && t <= upperBound).ToList();
    }

    private double CalcMean(List<long> times) => times.Average();

    private double CalcStd(List<long> times)
    {
        double mean = CalcMean(times);
        double sum = times.Sum(i => (i - mean) * (i - mean));
        double variance = sum / (times.Count - 1);
        double std = Math.Sqrt(variance);
        return std;
    }

    private (double Left, double Right) CalcConf95(List<long> times)
    {
        double mean = CalcMean(times);
        double z = 1.96;
        double pm = z * CalcStd(times) / Math.Sqrt(times.Count);

        return (mean - pm, mean + pm);
    }

    public string Report() => $"Total: {Total} +- {StdErr}, Mean: {Mean}, Std: {Std}, 95% CI: ({Conf95.Left}, {Conf95.Right})";

    public string ReportMs()
    {
        double factor = 1000.0 / Stopwatch.Frequency;
        return $"Mean: {Mean * factor} +- {StdErr * factor}ms, Total: {Total * factor}ms, Std: {Std * factor}ms, 95% CI: ({Conf95.Left * factor}ms, {Conf95.Right * factor}ms)";
    }
}
