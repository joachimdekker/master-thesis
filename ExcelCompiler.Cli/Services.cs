using ExcelCompiler.Config;
using ExcelCompiler.Extraction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        builder.AddScoped<ComputeModelExtractor>();
    }
}