using System.Diagnostics;
using ExcelCompiler.Cli.Config;
using ExcelCompiler.Generators;
using ExcelCompiler.Representations.CodeLayout;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ProjectCreationWorker
{
    private readonly ProjectGenerator _projectGenerator;
    private readonly RoslynGenerator _programGenerator;
    private readonly OutputConfiguration _configuration;
    private readonly ILogger<ProjectCreationWorker> _logger;

    public ProjectCreationWorker(ProjectGenerator projectGenerator, IOptions<OutputConfiguration> configuration, RoslynGenerator programGenerator, ILogger<ProjectCreationWorker> logger)
    {
        _projectGenerator = projectGenerator;
        _programGenerator = programGenerator;
        _logger = logger;
        _configuration = configuration.Value;
    }
    
    public async Task ExecuteAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        // Create the project
        Stopwatch sw = new();
        sw.Start();
        
        DirectoryInfo directory = new DirectoryInfo(_configuration.Location);
        await _projectGenerator.Generate(directory);
        
        // Create the main file
        await _programGenerator.Generate(project, _configuration.Location);

        sw.Stop();
        
        _logger.LogInformation("Project generation took {Time} ms", sw.ElapsedMilliseconds);
    }
}