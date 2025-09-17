using ExcelCompiler.Generated;
using ExcelCompiler.GeneratedProjectRunner;
using Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using System.Text.Json;

internal static class ActuarialSample
{
    public static async Task Main(string[] args)
    {
        ExcelCompiler.Generated.Program program = new();

        double total = 0;

        int count = 100_000;
        const string workbookPath = @"C:\Users\jdekk\Downloads\cp2022-p-scenarioset-20k-2025q3.xlsx";
        var outputDefintiion = new
        {
            Sheet = "VoorbeeldRTS",
            Cell = "I1"
        };
        string runId = Guid.NewGuid().ToString()[..8];

        Console.WriteLine("Generating inputs...");
        Stopwatch inputSw = Stopwatch.StartNew();
        int seed = 42;
        Random random = new(seed);
        List<Inputs> inputs = Enumerable.Range(0, count).Select(_ => Inputs.Create(random)).ToList();
        inputSw.Stop();
        Console.WriteLine($"Generated inputs in {inputSw.Elapsed.TotalMilliseconds} ms");


        Console.WriteLine("Starting Excel calculation test...");
        List<double> outputs = new();

        List<long> insertionTimes = new();
        List<long> calculationTimes = new();
        List<long> extractionTimes = new();

        Stopwatch wallTime = Stopwatch.StartNew();
        Stopwatch insertionTime = new();
        Stopwatch calculationTime = new();
        Stopwatch extractionTime = new();

        // Excelerate
        for (int i = 0; i < count; i++)
        {
            var input = inputs[i];
            calculationTime.Restart();
            var output = program.Main(input.Psi, input.Phi);
            calculationTime.Stop();

            insertionTimes.Add(0);
            calculationTimes.Add(calculationTime.ElapsedTicks);
            extractionTimes.Add(0);

            outputs.Add(output);

            if ((i + 1) % 5000 == 0) // Print less often, or every time if you want
                DrawProgressBar(i + 1, count);
        }
        wallTime.Stop();

        PerformanceData compiledPerformanceData = new(runId, workbookPath, wallTime.ElapsedTicks, insertionTimes, calculationTimes, extractionTimes);

        await compiledPerformanceData.Report("Excelerate");


        // Excel Interop
        var excel = new Microsoft.Office.Interop.Excel.Application
        {
            Visible = false,
            ScreenUpdating = false,
            DisplayAlerts = false,
            EnableEvents = false,
            Interactive = false,
        };
        var wb = excel.Workbooks.Open(workbookPath, ReadOnly: false);

        excel.CalculateBeforeSave = true;
        excel.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;

        List<double> excelOutputs = new();

        wallTime.Restart();
        calculationTimes.Clear();
        insertionTimes.Clear();
        extractionTimes.Clear();
        calculationTime.Reset();

        for (int i = 0; i < count; i++)
        {
            var inp = inputs[i];
            PutInputIntoSpreadsheet(inp, wb, insertionTime);

            // Do the real calculation
            calculationTime.Restart();
            excel.Calculate();
            calculationTime.Stop();

            // Grab output
            extractionTime.Restart();
            double outVal = Convert.ToDouble(wb.Sheets[outputDefintiion.Sheet].Range[outputDefintiion.Cell].Value2);
            extractionTime.Stop();

            calculationTimes.Add(calculationTime.ElapsedTicks);
            insertionTimes.Add(insertionTime.ElapsedTicks);
            extractionTimes.Add(extractionTime.ElapsedTicks);
            excelOutputs.Add(outVal);

            if ((i + 1) % 100 == 0) // Print less often, or every time if you want
                DrawProgressBar(i + 1, count);
        }
        wallTime.Stop();

        wb.Close(false);
        excel.Quit();

        PerformanceData excelPerformanceData = new(runId, workbookPath, wallTime.ElapsedTicks, insertionTimes, calculationTimes, extractionTimes);

        await excelPerformanceData.Report("Excel");

        // Check if every output is the same
        double tolerance = 1E-6;
        bool allEqual = true;
        for (int i = 0; i < count; i++)
        {
            if (Math.Abs(excelOutputs[i] - outputs[i]) > tolerance)
            {
                allEqual = false;
                Console.WriteLine($"Output mismatch at index {i}: {excelOutputs[i]} != {outputs[i]}");
            }
        }

        if (allEqual) Console.WriteLine("All outputs are equal!");

        // Comparisons
        Console.WriteLine("\n\nComparing performance data...");
        Console.WriteLine($"Speedup in Wall Time: {excelPerformanceData.WallTime / compiledPerformanceData.WallTime}x");
        Console.WriteLine($"Speedup in Calc Time: {excelPerformanceData.CalculationTime.Mean / compiledPerformanceData.CalculationTime.Mean}x");


        void PutInputIntoSpreadsheet(Inputs inp, Microsoft.Office.Interop.Excel.Workbook wb, Stopwatch sw)
        {
            // Phi
            object[,] phi = new object[inp.Phi.Count, 1];
            for (int i = 0; i < inp.Phi.Count; i++)
            {
                phi[i, 0] = inp.Phi[i].Column1;
            }

            // Psi
            object[,] psi = new object[inp.Psi.Count, 3];
            for (int i = 0; i < inp.Psi.Count; i++)
            {
                psi[i, 0] = inp.Psi[i].Column0;
                psi[i, 1] = inp.Psi[i].Column1;
                psi[i, 2] = inp.Psi[i].Column2;
            }

            // Write to spreadsheet
            sw.Start();
            wb.Sheets["Renteparameter_phi_R_NL"].Range["B1:B100"].Value2 = phi;
            wb.Sheets["Renteparameter_Psi_R"].Range["A1:C100"].Value2 = psi;
            sw.Stop();
        }

        void DrawProgressBar(int progress, int total, int barSize = 100)
        {
            double percent = (double)(progress) / total;
            int filled = (int)Math.Round(barSize * percent);
            string bar = new string('█', filled) + new string(' ', barSize - filled);
            string percentText = (percent * 100).ToString("0.0").PadLeft(5) + "%";
            Console.Write($"\r[{bar}] {percentText} ({progress}/{total})");
            if (progress == total)
                Console.WriteLine("\n"); // Move to next line at the end
        }
    }
}

file class Inputs
{
    public List<TableRenteparameter_phi_R_NLA1D100Item> Phi { get; set; }

    public List<TableRenteparameter_Psi_RA1C100Item> Psi { get; set; }

    public static Inputs Create(Random random)
    {
        var lengths = new
        {
            Phi = 100,
            Psi = 100,
        };

        var psi = Enumerable.Range(0, lengths.Psi).Select(_ => new TableRenteparameter_Psi_RA1C100Item(random.NextDouble(), random.NextDouble(), random.NextDouble())).ToList();
        var phi = Enumerable.Range(0, lengths.Phi).Select(_ => new TableRenteparameter_phi_R_NLA1D100Item(random.NextDouble())).ToList();

        return new Inputs()
        {
            Psi = psi,
            Phi = phi,
        };
    }
}