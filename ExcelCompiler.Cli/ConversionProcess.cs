using ExcelCompiler.Cli.Config;
using ExcelCompiler.Passes;
using ExcelCompiler.Passes.Compute;
using ExcelCompiler.Passes.Preview.Code;
using ExcelCompiler.Passes.Preview.Compute;
using ExcelCompiler.Passes.Structure;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ExtractRepositories = ExcelCompiler.Passes.Preview.Data.ExtractRepositories;
using Range = ExcelCompiler.Representations.References.Range;

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

    public async Task<Project> ExecuteAsync(ICollection<Location> singleInputs, ICollection<Range> structureInputs, ICollection<Location> outputs, CancellationToken cancellationToken = default)
    {
        // Get the Compiler Passes from the service provider

        // Structure
        ExcelToStructurePass excelToStructurePass = _serviceProvider.GetRequiredService<ExcelToStructurePass>();
        DetectAreas detectAreasPass = _serviceProvider.GetRequiredService<DetectAreas>();
        DetectStructures detectStructuresPass = _serviceProvider.GetRequiredService<DetectStructures>();
        
        // Compute
        Passes.Preview.StructureToComputePass structureToComputePass = _serviceProvider.GetRequiredService<Passes.Preview.StructureToComputePass>();
        ConstructComputeGraph constructComputeGraphPass = _serviceProvider.GetRequiredService<ConstructComputeGraph>();
        InsertInputs insertInputsPass = _serviceProvider.GetRequiredService<InsertInputs>();
        
        ExtractConstructs extractConstructsPass = _serviceProvider.GetRequiredService<ExtractConstructs>();
        ReplaceConstructDependencies replaceConstructDependenciesPass = _serviceProvider.GetRequiredService<ReplaceConstructDependencies>();
        Passes.Preview.ComputeToCodePass computeToCodePass = _serviceProvider.GetRequiredService<Passes.Preview.ComputeToCodePass>();
        TypeInference typeInferencePass = _serviceProvider.GetRequiredService<TypeInference>();
        PruneEmptyCells pruneEmptyCellsPass = _serviceProvider.GetRequiredService<PruneEmptyCells>();
        ExtractStructureData structureDataPass = _serviceProvider.GetRequiredService<ExtractStructureData>();
        InsertConstructs insertConstructsPass = _serviceProvider.GetRequiredService<InsertConstructs>();
        
        // Data
        ExtractRepositories extractRepositoriesPass = _serviceProvider.GetRequiredService<ExtractRepositories>();

        // Code
        InjectMemoization memoizationPass = _serviceProvider.GetRequiredService<InjectMemoization>();
        InsertStatements insertStatementsPass = _serviceProvider.GetRequiredService<InsertStatements>();
        InlineVariables inlineVariablesPass = _serviceProvider.GetRequiredService<InlineVariables>();
        
        // Open the stream
        _logger.LogInformation("Opening file {Location}", _options.Location);
        _logger.LogInformation("Starting Excel file processing for {Location}", _options.Location);
        Stream excelFile = File.OpenRead(_options.Location);

        // Extract the Excel Workbook
        _logger.LogInformation("Executing Excel to Structure pass");
        Workbook workbook = excelToStructurePass.Transform(excelFile);
        List<Area> areas = detectAreasPass.Detect(workbook).Where(a => !a.Range.IsSingleReference).ToList();
        List<Construct> constructs = detectStructuresPass.Detect(workbook, areas, structureInputs.ToList());
        workbook.Constructs.AddRange(constructs);
        
        // Extract data
        _logger.LogInformation("Executing Extract Repositories pass");
        var dataManager = extractRepositoriesPass.Transform(workbook);

        // Process the graph
        _logger.LogInformation("Executing Structure to Compute pass");
        var grid = structureToComputePass.Transform(workbook, outputs);
        grid = insertInputsPass.Transform(grid, singleInputs);
        _logger.LogInformation("Executing Extract Compute Tables pass");
        grid = extractConstructsPass.Generate(grid, constructs);
        grid = structureDataPass.Transform(grid);
        grid = insertConstructsPass.Transform(grid, constructs);
        
        _logger.LogInformation("Executing Construct Compute Graph pass");
        var graph = constructComputeGraphPass.Transform(grid, outputs.ToList());
        _logger.LogInformation("Inferencing Types");
        graph = typeInferencePass.Transform(graph);
        _logger.LogInformation("Executing Link Identical nodes pass");
        graph = replaceConstructDependenciesPass.Transform(graph);
        _logger.LogInformation("Executing Prune Empty Cells pass");
        graph = pruneEmptyCellsPass.Transform(graph);
        _logger.LogInformation("Completed all compiler passes successfully");
        // Transform to code (layout)
        var project = computeToCodePass.Transform(graph, dataManager);
        _logger.LogInformation("Executing memoization pass");
        project = memoizationPass.Transform(project);
        _logger.LogInformation("Executing inline variables pass");
        project = inlineVariablesPass.Transform(project);
        project = insertStatementsPass.Transform(project);
        
        return project;
    }
}
