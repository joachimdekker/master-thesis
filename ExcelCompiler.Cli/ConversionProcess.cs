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
    private readonly PruneEmptyCells _pruner;

    public ConversionWorker(ILogger<ConversionWorker> logger, IOptions<FileConfiguration> options, LinkDependencies linker, FrontendPass expandFunctionCompositions, PruneEmptyCells pruner)
    {
        _logger = logger;
        _extractor = expandFunctionCompositions;
        _pruner = pruner;
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
        
        // Extract the Excel Workbook
        Workbook workbook = _extractor.Transform(excelFile);
        
        // Temporary
        outputs = outputs.Select(o => o with {Spreadsheet = workbook.Spreadsheets[0].Name}).ToList();
        
        // Process the graph
        SupportGraph graph = _linker.Link(workbook, outputs);
        graph = _pruner.Transform(graph);
        _logger.LogInformation("Finished linking");

        return graph;
    }
}