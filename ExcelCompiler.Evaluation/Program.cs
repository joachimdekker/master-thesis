using System.Diagnostics;
using ExcelCompiler.Evaluation.Config;
using ExcelCompiler.Evaluation.Services;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using ExcelCompiler.Evaluation.Models;
using ExcelCompiler.Evaluation.Utils;
using Microsoft.Extensions.Logging;

// Top-level program setup
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

using ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddSimpleConsole(config =>
{
    config.SingleLine = true;
    config.IncludeScopes = false;
    config.TimestampFormat = "[HH:mm:ss] ";
}));
var logger = loggerFactory.CreateLogger<Program>();

var configuration = builder.Build();

// Bind config
var appConfig = configuration.Get<AppConfig>()!;

// Sanity checks
if (string.IsNullOrWhiteSpace(appConfig.SpreadsheetsRoot) || !Directory.Exists(appConfig.SpreadsheetsRoot))
{
    Console.WriteLine("SpreadsheetsRoot is missing or does not exist. Check appsettings.json.");
    return 1;
}

// EPPlus license context (make sure you comply with EPPlus license terms)
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var rng = new Random(appConfig.RandomSeed ?? 42);

// Services
var trialGenerator  = new TrialGenerator(rng);
var interopCalc     = new InteropCalculator();
var compiledInvoker = new TrialRunner(appConfig, rng, loggerFactory.CreateLogger<TrialRunner>());
var comparer        = new ResultComparer();

// Discover workbooks
var xlsxFiles = Directory.EnumerateFiles(appConfig.SpreadsheetsRoot, "*.xlsx", SearchOption.AllDirectories).ToList();
if (!xlsxFiles.Any()) throw new InvalidOperationException("No .xlsx files found in Spreadsheets folder (SpreadsheetsRoot)");

int total = 0, passed = 0;
var swAll = Stopwatch.StartNew();

foreach (var xlsx in xlsxFiles)
{
    Stopwatch sw = Stopwatch.StartNew();
    // Create the inputs
    IEnumerable<Trial> trials =
        Enumerable.Range(0, appConfig.TrialsPerWorkbook)
            .Select(_ => trialGenerator.GenerateTrial(xlsx));
    
    logger.LogInformation("Trials for {spreadsheet} generated after {duration}ms", xlsx, sw.ElapsedMilliseconds);
    
    /// For every trial, compile the workbook
    List<TrialRun> allRuns = [];
    foreach (var (i, trial) in trials.Index())
    {
        logger.LogInformation("Running trial {i} for {spreadsheet}: input {input}|output {output}", i, trial.SpreadsheetUrl, trial.InputCells[0].AddressWithSheet, trial.OutputCells[0].AddressWithSheet);
        Stopwatch swTrial = Stopwatch.StartNew();
        var results = await compiledInvoker.RunTrialAsync(trial);
        logger.LogInformation("Trial {i} for {spreadsheet} finished after {duration}ms", i, trial.SpreadsheetUrl, swTrial.ElapsedMilliseconds);
        allRuns.AddRange(results);
    }
    
    // Now for every ran trial, also run it by the Excel document and check if they are the same
    // They should be the same

    Dictionary<TrialRun, Dictionary<CellReference, object>> outputs = new();
    
    // Group the trialRuns per spreadsheet
    var grouped = allRuns.GroupBy(t => t.Trial.SpreadsheetUrl);
    foreach (var group in grouped)
    {
        var trialRuns = group.ToList();
        var outputsForSpreadsheet = interopCalc.CalculateForSpreadsheet(trialRuns);

        foreach (var (trialRun, output) in outputsForSpreadsheet)
        {
            outputs[trialRun] = output;
        }
    }
    
    // foreach (var trialRun in allRuns)
    // {
    //     var output = interopCalc.Calculate(trialRun);
    //     outputs[trialRun] = output;
    // }

    foreach (var (key, output) in outputs)
    {
        total++;
        if (!comparer.Compare(key.Results, output, appConfig.NumericTolerance, out var diffs))
        {
            logger.LogError("Results do not compare:\n{diffs}", diffs.Combine("\n"));
            continue;
        }
        passed++;
    }
    
    logger.LogInformation("Done for {spreadsheet} after {duration}ms.", xlsx, sw.ElapsedMilliseconds);
}

swAll.Stop();
Console.WriteLine($"\n=== Summary ===");
Console.WriteLine($"Tests: {passed}/{total} passed  |  Duration: {swAll.Elapsed}");
return passed == total && total > 0 ? 0 : 2;