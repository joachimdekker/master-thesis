﻿using ExcelCompiler.Cli;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
var project = await worker.ExecuteAsync([Location.FromA1("F7", "Monthly budget report"), ]);
    
// Run the project creation worker
ProjectCreationWorker projectWorker = provider.GetRequiredService<ProjectCreationWorker>();
await projectWorker.ExecuteAsync(project);
