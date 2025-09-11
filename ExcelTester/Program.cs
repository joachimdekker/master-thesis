// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;

// *** CONFIGURE ME ***
double[] inputs = [0.234, 0.015, 0.002567, 0.30, 0.003652d, 0.05d];
const string inputAddr  = "J6";
const string outputAddr = "E7";
const string workbookPath = @"C:\Users\joachimd\source\is\master-thesis\spreadsheets\Family budget monthly.xlsx";

// storage
var times   = new List<double>();
var outputs = new List<double>();
TimeSpan totalTime = TimeSpan.Zero;
TimeSpan totalInputTime = TimeSpan.Zero;
TimeSpan totalCalcTime = TimeSpan.Zero;
TimeSpan totalOutputTime = TimeSpan.Zero;

double totalOutput = 0;

// start Excel (hidden)
var excel = new Excel.Application 
{ 
    Visible = false,
    ScreenUpdating = false,
    DisplayAlerts = false,
    EnableEvents = false,
    Interactive = false,
};
var wb    = excel.Workbooks.Open(workbookPath, ReadOnly: false);
var ws    = (Excel.Worksheet)wb.Sheets[1];
var savingsSheet = (Excel.Worksheet)wb.Sheets["Interest"];

excel.CalculateBeforeSave = true;
excel.Calculation = Excel.XlCalculation.xlCalculationManual;

var outerSw = new Stopwatch();
var sw    = new Stopwatch();
var inputSw = new Stopwatch();
var outputSw = new Stopwatch();
Console.WriteLine("Starting Excel calculation test...");

// loop through inputs

outerSw.Start();
int count = 1_000_000;
for (int i = 0; i < count; i++)
{
    if (i % 1000 == 0)
    {
        Console.WriteLine($"Iteration {i} of {count} ({outerSw.Elapsed.TotalSeconds})");
    }

    // 1) inject
    inputSw.Restart();
    var inp = inputs[i % inputs.Length];
    savingsSheet.Range[inputAddr].Value2 = inp;
    inputSw.Stop();
    totalInputTime += inputSw.Elapsed;
    
    // 2) time the calc
    sw.Restart();
    excel.Calculate();
    sw.Stop();
    totalCalcTime += sw.Elapsed;

    // 3) grab output
    outputSw.Restart();
    double outVal = Convert.ToDouble(ws.Range[outputAddr].Value2);
    outputSw.Stop();
    totalOutputTime += outputSw.Elapsed;
    
    // 4) store
    totalTime   += sw.Elapsed;
    totalOutput += outVal;
}
outerSw.Stop();

// cleanup
wb.Close(false);
excel.Quit();

// Print results
Console.WriteLine($"Total time: {totalTime.TotalMilliseconds} ms");

// Print a summary of the times 
Console.WriteLine($"> Total input time: {totalInputTime.TotalMilliseconds} ms");
Console.WriteLine($"> Total calc time: {totalCalcTime.TotalMilliseconds} ms");
Console.WriteLine($"> Total output time: {totalOutputTime.TotalMilliseconds} ms");
Console.WriteLine(
    $"Sum of individual times; {(inputSw.Elapsed + sw.Elapsed + outputSw.Elapsed).TotalMilliseconds} ms (avg {(inputSw.Elapsed + sw.Elapsed + outputSw.Elapsed).TotalMilliseconds / count} ms)");
Console.WriteLine($"Average time: {totalTime.TotalMilliseconds / count} ms");
Console.WriteLine($"Total output: {totalOutput}");
Console.WriteLine($"Average output: {totalOutput / count}");
Console.WriteLine($"Walltime for {count} iterations: {outerSw.Elapsed.TotalSeconds} s / {outerSw.Elapsed.TotalMilliseconds} ms");