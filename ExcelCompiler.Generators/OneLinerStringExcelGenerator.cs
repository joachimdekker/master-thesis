using ExcelCompiler.Domain.Compute;
using Microsoft.Extensions.Logging;
using Range = ExcelCompiler.Domain.Compute.Range;

namespace ExcelCompiler.Generators;

public class OneLinerStringExcelGenerator : IFileGenerator
{
    private readonly ILogger<OneLinerStringExcelGenerator> _logger;
    
    public OneLinerStringExcelGenerator(ILogger<OneLinerStringExcelGenerator> logger)
    {
        _logger = logger;
    }

    public async Task Generate(SupportGraph graph, Stream outputStream, CancellationToken cancellationToken = default)
    {
        string code = "";
        
        // Create a variable per cell
        // Compute the roots in paralel
        foreach (ComputeUnit root in graph.Roots)
        {
            string? rootCode = GenerateCode(root);
            code += $"Console.WriteLine({rootCode});\n";
        }
        
        // Write the code to the file
        await WriteToFile(code, outputStream);
        
        _logger.LogInformation("Finished writing to file");
    }

    private string GenerateCode(ComputeUnit unit)
    {
        string code = unit switch
        {
            ConstantValue<string> constant => $"\"{constant.Value}\"",
            ConstantValue<decimal> constant => $"{constant.Value}",
            ConstantValue<int> constant => $"{constant.Value}",
            ConstantValue<double> constant => $"{constant.Value}",
            Reference reference => GenerateCode(reference.Dependencies[0]),
            Function { Name: "+" } function =>
                $"({string.Join(" + ", function.Dependencies.Select(GenerateCode))})",
            Function { Name: "-" } function =>
                $"({string.Join(" - ", function.Dependencies.Select(GenerateCode))})",
            Function { Name: "SUM", Dependencies: [Range range] } => $"(new int[] {{{string.Join(", ", range.Dependencies.Select(GenerateCode))}}}).Sum()",
            _ => throw new NotImplementedException($"Unknown type {unit.GetType()}")
        };

        return code;
    }

    public async Task WriteToFile(string code, Stream outputStream)
    {
        await using StreamWriter writer = new StreamWriter(outputStream);
        await writer.WriteAsync($$"""

                                  using System;
                                  using System.Collections.Generic;
                                  using System.Linq;
                                  using System.Text;
                                  using System.Threading.Tasks;

                                  namespace ExcelProgram;

                                  public static class Program 
                                  {
                                  public static void Main(string[] args)
                                  {
                                  {{code}}
                                  }
                                  }

                                  """);
    }
}