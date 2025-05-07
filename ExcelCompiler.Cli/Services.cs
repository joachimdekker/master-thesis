using System.Reflection;
using ExcelCompiler.Cli.Config;
using ExcelCompiler.Generators;
using ExcelCompiler.Passes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExcelCompiler.Cli;

public static class Services
{
    public static void AddNamedConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileConfiguration>(configuration.GetSection("File"));
        services.Configure<OutputConfiguration>(configuration.GetSection("Output"));
    }
    
    public static void AddServices(this IServiceCollection services)
    {
        // High Level Workers
        services.AddScoped<ConversionWorker>();
        services.AddScoped<ProjectCreationWorker>();
        
        // Get all subclasses of ICompilerPass and add them to the service collection.
        var passes = typeof(CompilerPassAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && 
                t.GetCustomAttribute<CompilerPassAttribute>() != null);
        
        foreach (var pass in passes)
        {
            services.AddScoped(pass);
        }
        
        // Generation
        //builder.AddScoped<IFileGenerator, OneLinerStringExcelGenerator>();
        services.AddScoped<IFileGenerator, RoslynSimpleGenerator>();
        services.AddScoped<ProjectGenerator>();
    }
}