using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ExcelCompiler.Generators;

public class FileWriter
{
    public FileWriter()
    {
        
    }
    
    public async Task Write(string path, List<CompilationUnitSyntax> units)
    {
        foreach (var unit in units)
        {
            // Get the name of the class in the compilation unit.
            // If there are multiple, always take the first one.
            var className = unit.Members.OfType<ClassDeclarationSyntax>().First().Identifier.Text;
            
            // Create the file path
            var filePath = Path.Combine(path, $"{className}.cs");
            
            // Write the file
            await using var writer = File.CreateText(filePath);
            await writer.WriteAsync(unit.NormalizeWhitespace().ToFullString());
        }
    }
}