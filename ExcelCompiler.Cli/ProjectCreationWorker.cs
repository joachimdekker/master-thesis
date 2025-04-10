using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Generators;
using Microsoft.Extensions.Options;

namespace ExcelCompiler.Cli;

public class ProjectCreationWorker
{
    private readonly ProjectGenerator _projectGenerator;
    private readonly OneLinerStringExcelGenerator _programGenerator;
    private readonly OutputConfiguration _configuration;


    public ProjectCreationWorker(ProjectGenerator projectGenerator, IOptions<OutputConfiguration> configuration, OneLinerStringExcelGenerator oneLinerStringExcelGenerator)
    {
        _projectGenerator = projectGenerator;
        _programGenerator = oneLinerStringExcelGenerator;
        _configuration = configuration.Value;
    }
    
    public async Task ExecuteAsync(SupportGraph graph, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        // Create the project
        DirectoryInfo directory = new DirectoryInfo(_configuration.Location);
        await _projectGenerator.Generate(directory);
        
        // Create the main file
        string projectFilePath = Path.Combine(directory.FullName, "Main.cs");
        Stream mainFile = File.Create(projectFilePath);
        await _programGenerator.GenerateFile(graph, mainFile);
    }
}