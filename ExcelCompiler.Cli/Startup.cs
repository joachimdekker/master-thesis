using ExcelCompiler.Cli;
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
services.AddSingleton<ConversionWorker>();

IServiceProvider provider = services.BuildServiceProvider();

// Run the worker
ConversionWorker worker = provider.GetRequiredService<ConversionWorker>();
await worker.ExecuteAsync();
