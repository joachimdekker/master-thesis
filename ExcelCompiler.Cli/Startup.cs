using ExcelCompiler.Cli;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Spreadsheet;
using ExcelCompiler.Generators;
using ExcelCompiler.Transformations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Get the config files
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Configure the service provider
IServiceCollection services = new ServiceCollection();
services.AddLogging(lb => lb.AddSimpleConsole().SetMinimumLevel(LogLevel.Trace));
services.AddNamedConfiguration(config);
services.AddServices();

IServiceProvider provider = services.BuildServiceProvider();

// Run the worker
ConversionWorker worker = provider.GetRequiredService<ConversionWorker>();
SupportGraph graph = await worker.ExecuteAsync([Location.FromA1("F17"), ]);

// Run the project creation worker
ProjectCreationWorker projectWorker = provider.GetRequiredService<ProjectCreationWorker>();
await projectWorker.ExecuteAsync(graph);
