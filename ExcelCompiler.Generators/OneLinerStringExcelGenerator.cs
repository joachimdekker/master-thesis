using ExcelCompiler.Domain.Compute;
using Microsoft.Extensions.Logging;

namespace ExcelCompiler.Generators;

public class OneLinerStringExcelGenerator
{
    private readonly ILogger<OneLinerStringExcelGenerator> _logger;
    
    public OneLinerStringExcelGenerator(ILogger<OneLinerStringExcelGenerator> logger)
    {
        _logger = logger;
    }

    public async Task GenerateFile(SupportGraph graph, Stream outputStream)
    {
        string code = "";
        
        // Create a variable per cell
        // Compute the roots in paralel
        foreach (ComputeUnit root in graph.Roots)
        {
            string? rootCode = string.Join("", root.Dependencies.Select(GenerateCode));
            code += rootCode;
        }
        
        // Write the code to the file
        await WriteToFile(code, outputStream);
        
        _logger.LogInformation("Finished writing to file");
    }

    private string GenerateCode(ComputeUnit unit)
    {
        string code = unit switch
        {
            ConstantValue<string> constant => $"\"{constant.Value}\";",
            ConstantValue<int> constant => $"{constant.Value};",
            ConstantValue<double> constant => $"{constant.Value};",
            FunctionComposition function => string.Join(" + ", function.Arguments.Select(GenerateCode)),
            _ => throw new NotImplementedException($"Unknown type {unit.GetType()}")
        };

        return code;
    }

    public async Task WriteToFile(string code, Stream outputStream)
    {
        await using StreamWriter writer = new StreamWriter(outputStream);
        await writer.WriteAsync($@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelProgram;

public class Program 
{{
public void Main(string[] args)
{{
{code}
}}
}}
");
    }
}