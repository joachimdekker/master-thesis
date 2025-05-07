using ExcelCompiler.Cli.Config;
using ExcelCompiler.Domain.Structure;
using ExcelCompiler.Passes;
using ExcelCompiler.Passes.Compute;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ConversionWorker
{
    private readonly ILogger<ConversionWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    private readonly FileConfiguration _options;

    public ConversionWorker(ILogger<ConversionWorker> logger, IServiceProvider serviceProvider, IOptions<FileConfiguration> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public async Task<SupportGraph> ExecuteAsync(ICollection<Location> outputs, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cancellation requested");
            return null!;
        }
        
        // Get the Compiler Passes from the service provider
        
        // Structure
        ExcellToStructurePass excelToStructurePass = _serviceProvider.GetRequiredService<ExcellToStructurePass>();
        
        // Compute
        StructureToComputePass structureToComputePass = _serviceProvider.GetRequiredService<StructureToComputePass>();
        ConstructComputeGraph constructComputeGraphPass = _serviceProvider.GetRequiredService<ConstructComputeGraph>();
        PruneEmptyCells pruneEmptyCellsPass = _serviceProvider.GetRequiredService<PruneEmptyCells>();
        ExtractComputeTables extractComputeTablesPass = _serviceProvider.GetRequiredService<ExtractComputeTables>();
        
        // Open the stream
        _logger.LogInformation("Opening file {Location}", _options.Location);
        Stream excelFile = File.OpenRead(_options.Location);
        
        // Extract the Excel Workbook
        Workbook workbook = excelToStructurePass.Transform(excelFile);
        
        // Process the graph
        var units = structureToComputePass.Transform(workbook, outputs);
        var tables = extractComputeTablesPass.Transform(workbook, units);
        var graph = constructComputeGraphPass.Transform(units, tables, outputs.ToList());
        graph = pruneEmptyCellsPass.Transform(graph);
        _logger.LogInformation("Finished linking");

        return graph;
    }
}