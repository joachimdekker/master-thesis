using ExcelCompiler.Cli.Config;
using ExcelCompiler.Generators;
using ExcelCompiler.Passes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExcelCompiler.Cli;

public static class Services
{
    public static void AddNamedConfiguration(this IServiceCollection builder, IConfiguration configuration)
    {
        builder.Configure<FileConfiguration>(configuration.GetSection("File"));
        builder.Configure<OutputConfiguration>(configuration.GetSection("Output"));
    }
    
    public static void AddServices(this IServiceCollection builder)
    {
        
        builder.AddScoped<ConversionWorker>();
        builder.AddScoped<ProjectCreationWorker>();

        builder.AddScoped<FrontendPass>();
        builder.AddScoped<LinkDependencies>();
        
        // Generation
        //builder.AddScoped<IFileGenerator, OneLinerStringExcelGenerator>();
        builder.AddScoped<IFileGenerator, RoslynSimpleGenerator>();
        builder.AddScoped<ProjectGenerator>();
    }
}