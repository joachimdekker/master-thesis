using Excel = Microsoft.Office.Interop.Excel;
using ExcelCompiler.Evaluation.Models;
using ExcelCompiler.Evaluation.Utils;

namespace ExcelCompiler.Evaluation.Services;

public sealed class InteropCalculator
{
    public Dictionary<CellReference, object> Calculate(TrialRun run)
    {
        var app = new Excel.Application
        {
            Visible = false,
            ScreenUpdating = false,
            DisplayAlerts = false
        };
        var wbs = app.Workbooks;
        var wb = wbs.Open(run.Trial.SpreadsheetUrl);

        app.CalculateBeforeSave = false;
        app.Calculation = Excel.XlCalculation.xlCalculationManual;
        
        try
        {
            // Set inputs
            foreach (var kvp in run.Inputs)
            {
                var (sheetName, address) = SplitAddress(kvp.Key);
                var ws = (Excel.Worksheet) wb.Worksheets[sheetName];
                var range = ws.Range[address];
                SetRangeValue(range, kvp.Value);
                ComHelpers.Release(range);
                ComHelpers.Release(ws);
            }

            // Calculate
            app.Calculate();

            // Read outputs
            var result = new Dictionary<CellReference, object>();
            foreach (var o in run.Trial.OutputCells)
            {
                var ws = (Excel.Worksheet)wb.Worksheets[o.SheetName];
                var range = ws.Range[o.Address];
                object? v = range.Value2;
                result[o] = v ?? "";
                ComHelpers.Release(range);
                ComHelpers.Release(ws);
            }

            return result;
        }
        finally
        {
            try { wb.Close(SaveChanges: false); } finally { ComHelpers.Release(wb); }
            ComHelpers.Release(wbs);
            try { app.Quit(); } finally { ComHelpers.Release(app); }
        }
    }


    public Dictionary<TrialRun, Dictionary<CellReference, object>> CalculateForSpreadsheet(ICollection<TrialRun> trials)
    {
        // Check if the trials are all for the same spreadsheet
        if (trials.Count < 1) throw new ArgumentException("Trials must not be empty");
        var spreadsheetUrl = trials.First().Trial.SpreadsheetUrl;
        if (trials.Any(t => t.Trial.SpreadsheetUrl != spreadsheetUrl)) throw new ArgumentException("Trials must all be for the same spreadsheet");
        
        // Open the spreadsheet
        var app = new Excel.Application
        {
            Visible = false,
            ScreenUpdating = false,
            DisplayAlerts = false
        };
        var wbs = app.Workbooks;
        var workbook = wbs.Open(spreadsheetUrl);

        app.CalculateBeforeSave = false;
        app.Calculation = Excel.XlCalculation.xlCalculationManual;

        var results = new Dictionary<TrialRun, Dictionary<CellReference, object>>();
        try
        {
            // Group the trialRuns per trial
            var grouped = trials.GroupBy(t => t.Trial);
            foreach (var group in grouped)
            {
                var trial = group.Key;

                // Collect the values of the inputs
                var inputs = new Dictionary<string, object>();
                foreach (var input in trial.InputCells)
                {
                    var ws = (Excel.Worksheet)workbook.Worksheets[input.SheetName];
                    var range = ws.Range[input.Address];
                    object? v = range.Value2;
                    inputs[input.AddressWithSheet] = v ?? "";
                    ComHelpers.Release(range);
                    ComHelpers.Release(ws);
                }

                // Run the trials
                foreach (var trialRun in group)
                {
                    // Set inputs
                    foreach (var kvp in trialRun.Inputs)
                    {
                        var (sheetName, address) = SplitAddress(kvp.Key);
                        var ws = (Excel.Worksheet)workbook.Worksheets[sheetName];
                        var range = ws.Range[address];
                        SetRangeValue(range, kvp.Value);
                        ComHelpers.Release(range);
                        ComHelpers.Release(ws);
                    }

                    // Calculate
                    Console.WriteLine("Calculating...");
                    app.Calculate();

                    // Read outputs
                    var result = new Dictionary<CellReference, object>();
                    foreach (var o in trialRun.Trial.OutputCells)
                    {
                        var ws = (Excel.Worksheet)workbook.Worksheets[o.SheetName];
                        var range = ws.Range[o.Address];
                        object? v = range.Value2;
                        result[o] = v ?? "";
                        ComHelpers.Release(range);
                        ComHelpers.Release(ws);
                    }

                    results[trialRun] = result;
                }

                // Set the inputs back to their original values
                foreach (var (address, value) in inputs)
                {
                    var (sheetName, _) = SplitAddress(address);
                    var ws = (Excel.Worksheet)workbook.Worksheets[sheetName];
                    var range = ws.Range[address];
                    SetRangeValue(range, value);
                    ComHelpers.Release(range);
                }
            }
        }
        finally
        {
            // Close the workbook
            try { workbook.Close(SaveChanges: false); } finally { ComHelpers.Release(workbook); }
            ComHelpers.Release(wbs);
            try { app.Quit(); } finally { ComHelpers.Release(app); }
        }
        
        return results;
    }


    private static (string sheet, string address) SplitAddress(string sheetBangAddress)
    {
        var idx = sheetBangAddress.IndexOf('!');
        if (idx < 0) throw new ArgumentException($"Invalid A1 address (expected Sheet!A1): {sheetBangAddress}");
        return (sheetBangAddress[1..(idx-1)], sheetBangAddress[(idx + 1)..]);
    }

    private static void SetRangeValue(Excel.Range range, object value)
    {
        range.Value2 = value switch
        {
            bool or double => value,
            float or decimal or int or long or short or byte => (double)value,
            _ => value?.ToString() ?? ""
        };
    }
}
