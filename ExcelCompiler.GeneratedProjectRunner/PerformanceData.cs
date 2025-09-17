using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

public record PerformanceData
{
    public required string RunId { get; init; }
    public required string WorkbookPath { get; init; }

    public required int Count { get; init; }
    public required long WallTime { get; init; }

    public DataSummary InsertionTime { get; }
    public DataSummary CalculationTime { get; }
    public DataSummary ExtractionTime { get; }

    public required List<long> InsertionTimes { get; set; }
    public required List<long> CalculationTimes { get; set; }
    public required List<long> ExtractionTimes { get; set; }

    [SetsRequiredMembers]
    public PerformanceData(string runId, string workbook, long wallTime, List<long> insertionTimes, List<long> calculationTimes, List<long> extractionTimes)
    {
        if (insertionTimes.Count != calculationTimes.Count || insertionTimes.Count != extractionTimes.Count)
            throw new ArgumentException("All time lists must have the same length.");

        // Remove the first couple of entries to avoid warm-up effects
        int warmup = Math.Min(5, insertionTimes.Count / 10);
        
        RunId = runId;
        WorkbookPath = workbook;
        Count = insertionTimes.Count;

        WallTime = wallTime;
        
        InsertionTimes = insertionTimes;
        CalculationTimes = calculationTimes;
        ExtractionTimes = extractionTimes;

        InsertionTime = new(InsertionTimes.Skip(warmup).ToList());
        CalculationTime = new(CalculationTimes.Skip(warmup).ToList());
        ExtractionTime = new(ExtractionTimes.Skip(warmup).ToList());
    }

    public async Task Report(string title = "Performance")
    {
        Console.WriteLine("--{  Performance Report  }--");
        Console.WriteLine($"Title: {title}");
        Console.WriteLine($"Count: {Count}");
        Console.WriteLine($"Total time: {WallTime * 1000d / Stopwatch.Frequency}ms");
        Console.WriteLine($"  >  Insertion:   {InsertionTime.ReportMs()}");
        Console.WriteLine($"  >  Calculation: {CalculationTime.ReportMs()}");
        Console.WriteLine($"  >  Output:      {ExtractionTime.ReportMs()}");
        Console.WriteLine();
    
        // Write data to file in json
        // Get the path, based on the date and the title
        string path = Path.Combine(@"C:\Users\jdekk\source\uva\master-thesis", "performance-reports", $"{DateTime.Now:yyyy-MM-dd}", $"{title}-{DateTime.Now:HH-m-s}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    
        await using var file = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(file, this);
    
        Console.WriteLine($"Performance data written to {path}");
        Console.WriteLine("-{}-{}-{}-{}-{}-{}-{}-{}-{}-");
    }
}