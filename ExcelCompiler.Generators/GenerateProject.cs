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
        string projectFileContent = await File.ReadAllTextAsync("static/csproj.xml");
        await File.WriteAllTextAsync(projectFilePath, projectFileContent);
    }
}