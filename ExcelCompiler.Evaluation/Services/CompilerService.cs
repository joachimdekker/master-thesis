using System.Diagnostics;
using ExcelCompiler.Evaluation.Config;
using ExcelCompiler.Evaluation.Models;
using ExcelCompiler.Evaluation.Utils;

namespace ExcelCompiler.Evaluation.Services;

public sealed class CompilerService(AppConfig cfg)
{
    public async Task<string> CompileAsync(Trial trial, CancellationToken ct = default)
    {
        var dir = Path.Combine(Path.GetTempPath(), "excel-compiler-eval", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);

        string[] args =
        [
            $"Output:Location=\"{dir}\"",
            $"File:Location=\"{trial.SpreadsheetUrl}\"",
            ..trial.InputCells.Select((c, i) => $"File:Inputs:{i}=\"{c.AddressWithSheet}\""),
            ..trial.OutputCells.Select((c, i) => $"File:Outputs:{i}=\"{c.AddressWithSheet}\""),
        ];

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {cfg.ToolProject} -- {args.Combine()}",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow = true
        };

        using var proc = new Process { StartInfo = psi };
        if (!proc.Start()) throw new InvalidOperationException("Failed to start process.");

        string stdout = await proc.StandardOutput.ReadToEndAsync(ct);
        string stderr = await proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);

        if (proc.ExitCode != 0)
        {
            Console.WriteLine("Compilation failed.");
            if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout);
            if (!string.IsNullOrWhiteSpace(stderr)) Console.WriteLine(stderr);

            throw new InvalidOperationException("Compilation failed.");
        }
        
        // Build the compiled project
        var buildProcessInfo = new ProcessStartInfo
        {
            WorkingDirectory = dir,
            FileName = "dotnet",
            Arguments = $"build -c Release",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            CreateNoWindow = true
        };
        
        using var buildProc = new Process { StartInfo = buildProcessInfo };
        if (!buildProc.Start()) throw new InvalidOperationException("Failed to start process.");
        
        await buildProc.WaitForExitAsync(ct);
        stdout = await buildProc.StandardOutput.ReadToEndAsync(ct);
        stderr = await buildProc.StandardError.ReadToEndAsync(ct);

        if (buildProc.ExitCode != 0)
        {
            Console.WriteLine("Build failed.");
            if (!string.IsNullOrWhiteSpace(stdout)) Console.WriteLine(stdout);
            if (!string.IsNullOrWhiteSpace(stderr)) Console.WriteLine(stderr);
            throw new InvalidOperationException("Build failed.");       
        }

        // Newest DLL under outDir
        var binPath = Path.Combine(dir, "bin", "Release");
        var dll = Directory.EnumerateFiles(binPath, "*.dll", SearchOption.AllDirectories)
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault(f => f.Contains("ExcelCompiler"));
        
        return dll ?? throw new InvalidOperationException("No DLL found.");
    }
}