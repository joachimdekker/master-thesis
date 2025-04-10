using ExcelCompiler.Cli.Config;
using ExcelCompiler.Domain;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Spreadsheet;
using ExcelCompiler.Extraction;
using ExcelCompiler.Transformations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ConversionWorker
{
    private readonly ILogger<ConversionWorker> _logger;
    private readonly ComputeModelExtractor _extractor;
    private readonly FileConfiguration _options;
    private readonly LinkDependencies _linker;
    private readonly ExpandFunctionCompositions _expandFunctionCompositions;

    public ConversionWorker(ILogger<ConversionWorker> logger, IOptions<FileConfiguration> options, ComputeModelExtractor extractor, LinkDependencies linker, ExpandFunctionCompositions expandFunctionCompositions)
    {
        _logger = logger;
        _extractor = extractor;
        _linker = linker;
        _expandFunctionCompositions = expandFunctionCompositions;
        _options = options.Value;
    }

    public async Task<SupportGraph> ExecuteAsync(ICollection<Location> outputs, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cancellation requested");
            return null!;
        }
        
        // Open the stream
        _logger.LogInformation("Opening file {Location}", _options.Location);
        Stream excelFile = File.OpenRead(_options.Location);
        
        List<ComputeUnit> units = _extractor.Extract(excelFile, outputs);
        _logger.LogInformation("Finished extracting cells");
        
        // Process the graph
        units = _expandFunctionCompositions.Transform(units);
        SupportGraph graph = _linker.Link(units, outputs);

        return graph;
    }
}