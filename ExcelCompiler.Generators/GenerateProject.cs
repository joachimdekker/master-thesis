namespace ExcelCompiler.Generators;

public class ProjectGenerator
{
    public async Task Generate(DirectoryInfo directory)
    {
        // Create the directory if it does not exist
        directory.Create();
        
        // Delete every file in the directory
        foreach (var file in directory.GetFiles())
        {
            file.Delete();
        }
        
        // Create the project file
        // Open the project file from static/csproj.xml and copy it to the directory
        string projectFilePath = Path.Combine(directory.FullName, "ExcelCompiler.csproj");
        
        // Get the path to the assembly containing the static csproj file
        // This is needed because the tool may be run from a different directory
        string assemblyDir = Path.GetDirectoryName(typeof(ProjectGenerator).Assembly.Location)!;
        string staticCsprojPath = Path.Combine(assemblyDir, "static", "csproj.xml");

        string projectFileContent = await File.ReadAllTextAsync(staticCsprojPath);
        await File.WriteAllTextAsync(projectFilePath, projectFileContent);
    }
}