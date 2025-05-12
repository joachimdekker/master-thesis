using ExcelCompiler.Cli.Config;
using ExcelCompiler.Generators;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Data;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ProjectCreationWorker
{
    private readonly ProjectGenerator _projectGenerator;
    private readonly RoslynGenerator _programGenerator;
    private readonly OutputConfiguration _configuration;

    public ProjectCreationWorker(ProjectGenerator projectGenerator, IOptions<OutputConfiguration> configuration, RoslynGenerator programGenerator)
    {
        _projectGenerator = projectGenerator;
        _programGenerator = programGenerator;
        _configuration = configuration.Value;
    }
    
    public async Task ExecuteAsync(Project project, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        // Create the project
        DirectoryInfo directory = new DirectoryInfo(_configuration.Location);
        await _projectGenerator.Generate(directory);
        
        // Create the main file
        await _programGenerator.Generate(project, _configuration.Location);
    }
}