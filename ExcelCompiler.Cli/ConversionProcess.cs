using ExcelCompiler.Cli.Config;
using ExcelCompiler.Passes;
using ExcelCompiler.Passes.Compute;
using ExcelCompiler.Passes.Data;
using ExcelCompiler.Passes.Structure;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.Data;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExtractRepositories = ExcelCompiler.Passes.Preview.Data.ExtractRepositories;

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

    public async Task<Project> ExecuteAsync(ICollection<Location> outputs, CancellationToken cancellationToken = default)
    {
        // Get the Compiler Passes from the service provider

        // Structure
        ExcelToStructurePass excelToStructurePass = _serviceProvider.GetRequiredService<ExcelToStructurePass>();
        DetectAreas detectAreasPass = _serviceProvider.GetRequiredService<DetectAreas>();
        DetectStructures detectStructuresPass = _serviceProvider.GetRequiredService<DetectStructures>();
        
        // Compute
        StructureToComputePass structureToComputePass = _serviceProvider.GetRequiredService<StructureToComputePass>();
        ConstructComputeGraph constructComputeGraphPass = _serviceProvider.GetRequiredService<ConstructComputeGraph>();
        TypeInference typeInferencePass = _serviceProvider.GetRequiredService<TypeInference>();
        PruneEmptyCells pruneEmptyCellsPass = _serviceProvider.GetRequiredService<PruneEmptyCells>();
        ExtractComputeTables extractComputeTablesPass = _serviceProvider.GetRequiredService<ExtractComputeTables>();

        // Data
        ExtractRepositories extractRepositoriesPass = _serviceProvider.GetRequiredService<ExtractRepositories>();

        // Code Layout
        ComputeToCodePass computeToCodePass = _serviceProvider.GetRequiredService<ComputeToCodePass>();

        // Open the stream
        _logger.LogInformation("Opening file {Location}", _options.Location);
        _logger.LogInformation("Starting Excel file processing for {Location}", _options.Location);
        Stream excelFile = File.OpenRead(_options.Location);

        // Extract the Excel Workbook
        _logger.LogInformation("Executing Excel to Structure pass");
        Workbook workbook = excelToStructurePass.Transform(excelFile);
        List<Area> areas = detectAreasPass.Detect(workbook).Where(a => !a.Range.IsSingleReference).ToList();
        List<Construct> constructs = detectStructuresPass.Detect(workbook, areas);
        workbook.Constructs.AddRange(constructs);
        
        // Extract data
        _logger.LogInformation("Executing Extract Repositories pass");
        var dataManager = extractRepositoriesPass.Transform(workbook);

        // Process the graph
        _logger.LogInformation("Executing Structure to Compute pass");
        var units = structureToComputePass.Transform(workbook, outputs);
        _logger.LogInformation("Executing Extract Compute Tables pass");
        var tables = extractComputeTablesPass.Transform(workbook, units);
        _logger.LogInformation("Executing Construct Compute Graph pass");
        var graph = constructComputeGraphPass.Transform(units, tables, outputs.ToList());
        graph = typeInferencePass.Transform(graph);
        _logger.LogInformation("Executing Prune Empty Cells pass");
        graph = pruneEmptyCellsPass.Transform(graph);
        _logger.LogInformation("Completed all compiler passes successfully");

        // Transform to code (layout)
        var project = computeToCodePass.Transform(graph, dataManager);

        return project;
    }
}
