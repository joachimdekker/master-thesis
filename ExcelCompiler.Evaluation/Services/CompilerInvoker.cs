using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using ExcelCompiler.Evaluation.Config;
using ExcelCompiler.Evaluation.Models;
using Microsoft.Extensions.Logging;

namespace ExcelCompiler.Evaluation.Services;

public sealed class TrialRunner(AppConfig cfg, Random rng, ILogger<TrialRunner> logger)
{
    private CompilerService _compiler = new(cfg);
    
    public MethodInfo FindEntryPoint(AssemblyLoadContext ctx, string compiledDllPath)
    {
        var asm = ctx.LoadFromAssemblyPath(compiledDllPath);

        var program = asm.DefinedTypes.FirstOrDefault(t => t.Name == "Program");
        var method = program?.DeclaredMethods.FirstOrDefault(m => m.Name == "Main");
        
        return method ?? throw new InvalidOperationException("No suitable entry method found.");
    }

    public async Task<List<TrialRun>> RunTrialAsync(Trial trial)
    {
        Stopwatch sw = Stopwatch.StartNew();
        var loadContext = new AssemblyLoadContext("Loader", true);
        var compiledDllPath = await _compiler.CompileAsync(trial);
        var entry = FindEntryPoint(loadContext, compiledDllPath);
        logger.LogInformation("Compilation took {Time} ms", sw.ElapsedMilliseconds);
        
        logger.LogInformation("Beginning trials for {spreadsheet}.", trial.SpreadsheetUrl);
        sw.Restart();
        var run =  Enumerable.Range(0, cfg.TestsPerTrial).Select(_ => RunTrial(entry, trial)).ToList();
        logger.LogInformation("Trials for {spreadsheet} finished after {duration}ms", trial.SpreadsheetUrl, sw.ElapsedMilliseconds);
        loadContext.Unload();
        return run;
    }
    
    public TrialRun RunTrial(MethodInfo entry, Trial trial)
    {
        // We assume the inputs are doubles
        var inputs = trial.InputCells.ToDictionary(i => i, _ => (object)rng.NextDouble());
        
        // Create the class with an empty construtor
        var constructor = entry.DeclaringType!.GetConstructor([])!;
        var instance = constructor.Invoke([]);
        
        // For now we just assume one output
        
        // If there is no input, that means that the input is not used.
        // In that case, we can just return the static value.
        var param = inputs.Values.Take(entry.GetParameters().Length).ToArray();
        var output = entry.Invoke(instance, param);

        // Create the TrialRun
        return new TrialRun()
        {
            Inputs = inputs,
            Results = trial.OutputCells.ToDictionary(i => i, _ => output!),
            Trial = trial,
        };
    }
}
