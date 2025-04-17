using ExcelCompiler.Cli.Config;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Structure;
using ExcelCompiler.Passes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ConversionWorker
{
    private readonly ILogger<ConversionWorker> _logger;
    private readonly FrontendPass _extractor;
    private readonly FileConfiguration _options;
    private readonly LinkDependencies _linker;

    public ConversionWorker(ILogger<ConversionWorker> logger, IOptions<FileConfiguration> options, LinkDependencies linker, FrontendPass expandFunctionCompositions)
    {
        _logger = logger;
        _extractor = expandFunctionCompositions;
        _linker = linker;
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
        
        var units = _extractor.Transform(excelFile);
        _logger.LogInformation("Finished extracting cells");
        
        // Process the graph
        SupportGraph graph = _linker.Link(units, outputs);

        return graph;
    }
}