using ExcelCompiler.Cli;
using ExcelCompiler.Cli.Config;
using ExcelCompiler.Representations.References;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using Range = ExcelCompiler.Representations.References.Range;

ExcelPackage.License.SetNonCommercialPersonal("Joachim Dekker");

// Get the config files
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

// Configure the service provider
IServiceCollection services = new ServiceCollection();
services.AddLogging(lb => lb.AddSimpleConsole(options =>
{
    options.IncludeScopes = true;
    options.SingleLine = true;
    options.TimestampFormat = "[HH:mm:ss] ";
    options.ColorBehavior = LoggerColorBehavior.Enabled;
}).SetMinimumLevel(LogLevel.Trace));    
services.AddNamedConfiguration(config);
services.AddServices();

IServiceProvider provider = services.BuildServiceProvider();

// Get the inputs and outputs

FileConfiguration fileConfig = provider.GetRequiredService<IOptions<FileConfiguration>>().Value;

if (fileConfig.Outputs is null || fileConfig.Outputs.Count == 0)
    throw new InvalidOperationException("No outputs found.");

var inputs = fileConfig.Inputs?.Select(i => Reference.Parse(i)).OfType<Location>().ToList() ?? [];
var structureInputs = fileConfig.StructureInputs?.Select(i => Reference.Parse(i)).OfType<Range>().ToList() ?? [];

var outputs = fileConfig.Outputs!.Select(i => Reference.Parse(i)).OfType<Location>().ToList();

// Run the worker
ConversionWorker worker = provider.GetRequiredService<ConversionWorker>();
var project = await worker.ExecuteAsync(
    inputs, //[Location.FromA1("E10", "Monthly budget report")], 
    structureInputs, //[Range.FromString("C14:F17", "Monthly budget report")], 
    outputs);
    
// Run the project creation worker
ProjectCreationWorker projectWorker = provider.GetRequiredService<ProjectCreationWorker>();
await projectWorker.ExecuteAsync(project);
