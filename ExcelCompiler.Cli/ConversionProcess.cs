using ExcelCompiler.Config;
using ExcelCompiler.Domain;
using ExcelCompiler.Domain.Spreadsheet;
using ExcelCompiler.Extraction;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ConversionWorker
{
    private readonly ILogger<ConversionWorker> _logger;
    private readonly ComputeModelExtractor _extractor;
    private readonly FileConfiguration _options;

    public ConversionWorker(ILogger<ConversionWorker> logger, IOptions<FileConfiguration> options, ComputeModelExtractor extractor)
    {
        _logger = logger;
        _extractor = extractor;
        _options = options.Value;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // Open the stream
        _logger.LogInformation("Opening file {Location}", _options.Location);
        Stream excelFile = File.OpenRead(_options.Location);
        List<Cell> cells = _extractor.Extract(excelFile);
        _logger.LogInformation("Finished extracting cells");
        
        foreach (var cell in cells)
        {
            Console.WriteLine(cell);
        }
        
        return Task.CompletedTask;
    }
}