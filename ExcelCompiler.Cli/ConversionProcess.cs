using ExcelCompiler.Config;
using ExcelCompiler.Domain;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Spreadsheet;
using ExcelCompiler.Extraction;
using ExcelCompiler.Generators;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ConversionWorker
{
    private readonly ILogger<ConversionWorker> _logger;
    private readonly ComputeModelExtractor _extractor;
    private readonly OneLinerStringExcelGenerator _generator;
    private readonly FileConfiguration _options;
    private readonly OutputConfiguration _outputOptions;

    public ConversionWorker(ILogger<ConversionWorker> logger, IOptions<FileConfiguration> options, ComputeModelExtractor extractor, OneLinerStringExcelGenerator generator, IOptions<OutputConfiguration> outputOptions)
    {
        _logger = logger;
        _extractor = extractor;
        _generator = generator;
        _outputOptions = outputOptions.Value;
        _options = options.Value;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cancellation requested");
            return;
        }
        
        // Open the stream
        _logger.LogInformation("Opening file {Location}", _options.Location);
        Stream excelFile = File.OpenRead(_options.Location);
        
        Location startCell = Location.FromA1("F17");
        SupportGraph graph = _extractor.Extract(excelFile, startCell);
        _logger.LogInformation("Finished extracting cells");
        
        // Process the graph
        
        
        // Create a csharp file
        Stream csharpFile = File.Create(_outputOptions.Location);
        await _generator.GenerateFile(graph, csharpFile);
    }
}