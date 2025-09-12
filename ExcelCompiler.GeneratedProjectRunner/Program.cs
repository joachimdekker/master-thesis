using ExcelCompiler.Generated;

using System.Diagnostics;
using System.Text.Json;
using ExcelCompiler.GeneratedProjectRunner;

internal class ServiceInvoice
{
    public static async Task Main(string[] args)
    {
        ExcelCompiler.Generated.Program program = new();

        double total = 0;

        int count = 1;
        const string workbookPath = @"C:\Users\joachimd\source\is\master-thesis\spreadsheets\Service invoice.xlsx";
        
        var outputDefintiion = new
        {
            Sheet = "Service invoice",
            Cell = "E29"
        };


        // Generate input
        Console.WriteLine("Generating inputs...");
        Stopwatch inputSw = Stopwatch.StartNew();
        int seed = 42;
        Random random = new(seed);
        List<Inputs> inputs = Enumerable.Range(0, count).Select(_ => Inputs.Create(random)).ToList();
        inputSw.Stop();
        Console.WriteLine($"Generated inputs in {inputSw.Elapsed.TotalMilliseconds} ms");


        Console.WriteLine("Starting Excel calculation test...");
        List<double> outputs = new();
        List<long> calculationTimes = new();

        Stopwatch wallTime = Stopwatch.StartNew();
        Stopwatch calculationTime = new();
        Stopwatch individualCalculationTime = new();
        for (int i = 0; i < count; i++)
        {
            var input = inputs[i];
            calculationTime.Start();
            individualCalculationTime.Restart();
            var output = program.Main(input.SalesTax, input.InvoiceDetails);
            individualCalculationTime.Stop();
            calculationTime.Stop();
    
            calculationTimes.Add(individualCalculationTime.ElapsedTicks);
            outputs.Add(output);
    
            if ((i + 1) % 5000 == 0) // Print less often, or every time if you want
                DrawProgressBar(i + 1, count);
        }
        wallTime.Stop();

        PerformanceData compiledPerformanceData = new()
        {
            WallTime = wallTime.Elapsed.TotalMilliseconds,
            CalculationTime = calculationTime.Elapsed.TotalMilliseconds,
            InsertionTime = 0,
            OutputTime = 0,
            CalculationTimes = calculationTimes,
            Count = count,
        };

        await compiledPerformanceData.Report("Excelerate");


        // Excel Interop
        Stopwatch inputInjection = new();
        Stopwatch outputExtraction = new();

        var excel = new Microsoft.Office.Interop.Excel.Application
        { 
            Visible = false,
            ScreenUpdating = false,
            DisplayAlerts = false,
            EnableEvents = false,
            Interactive = false,
        };
        var wb    = excel.Workbooks.Open(workbookPath, ReadOnly: false);

        excel.CalculateBeforeSave = true;
        excel.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;
        
        List<double> excelOutputs = new();

        wallTime.Restart();
        calculationTime.Reset();
        calculationTimes.Clear();
        individualCalculationTime.Reset();
        for (int i = 0; i < count; i++)
        {
            var inp = inputs[i];
            PutInputIntoSpreadsheet(inp, wb, inputInjection);
    
            // Do the real calculation
            calculationTime.Start();
            individualCalculationTime.Restart();
            excel.Calculate();
            individualCalculationTime.Stop();
            calculationTime.Stop();

            // Grab output
            outputExtraction.Start();
            double outVal = Convert.ToDouble(wb.Sheets[outputDefintiion.Sheet].Range[outputDefintiion.Cell].Value2);
            outputExtraction.Stop();
    
            calculationTimes.Add(individualCalculationTime.ElapsedTicks);
            excelOutputs.Add(outVal);
    
            if ((i + 1) % 100 == 0) // Print less often, or every time if you want
                DrawProgressBar(i + 1, count);
        }
        wallTime.Stop();

        wb.Close(false);
        excel.Quit();

        PerformanceData excelPerformanceData = new()
        {
            WallTime = wallTime.Elapsed.TotalMilliseconds,
            CalculationTime = calculationTime.Elapsed.TotalMilliseconds,
            InsertionTime = inputInjection.Elapsed.TotalMilliseconds,
            OutputTime = outputExtraction.Elapsed.TotalMilliseconds,
            Count = count,
            CalculationTimes = calculationTimes,
        };

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
        Console.WriteLine($"Speedup in Calc Time: {excelPerformanceData.CalculationTime / compiledPerformanceData.CalculationTime}x");


        void PutInputIntoSpreadsheet(Inputs inp, Microsoft.Office.Interop.Excel.Workbook wb, Stopwatch sw)
        {
            object[,] qty = new object[inp.InvoiceDetails.Count, 1];
            object[,] price = new object[inp.InvoiceDetails.Count, 1];
            for (int i = 0; i < inp.InvoiceDetails.Count; i++)
            {
                qty[i, 0] = inp.InvoiceDetails[i].Qty;
                price[i, 0] = inp.InvoiceDetails[i].UnitPrice;
            }
            
            // Write to spreadsheet
            sw.Start();
            wb.Sheets["Service invoice"].Range["B17:B25"].Value2 = qty;
            wb.Sheets["Service invoice"].Range["D17:D25"].Value2 = price;
            wb.Sheets["Service invoice"].Range["E28"].Value2 = (object)inp.SalesTax;
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
    public double SalesTax { get; set; }
    
    public List<InvoiceDetailsItem> InvoiceDetails { get; set; }

    public static Inputs Create(Random random)
    {
        var lengths = new
        {
            Items = 9,
        };
        
        var invoiceDetails = Enumerable.Range(0, lengths.Items).Select(_ => new InvoiceDetailsItem(random.Next(1, 50), random.Next(100, 200))).ToList();
        var salesTax = random.NextDouble(0.1, 0.2);
        
        return new Inputs()
        {
            InvoiceDetails = invoiceDetails,
            SalesTax = salesTax,
        };
    }
}

file record PerformanceData
{
    public required int Count { get; init; }
    
    public required double WallTime { get; init; }
    public required double InsertionTime { get; init; }
    public required double CalculationTime { get; init; }
    public required double OutputTime { get; init; }
    
    public double AverageOutputTime => OutputTime / Count;
    public double AverageCalculationTime => CalculationTime / Count;
    public double AverageInsertionTime => InsertionTime / Count;
    
    public required List<long> CalculationTimes { get; set; }
    
    public async Task Report(string title = "Performance")
    {
        Console.WriteLine("--{  Performance Report  }--");
        Console.WriteLine($"Title: {title}");
        Console.WriteLine($"Count: {Count}");
        Console.WriteLine($"Total time: {WallTime}ms");
        Console.WriteLine($"  >  Insertion:   {InsertionTime}ms");
        Console.WriteLine($"  >  Calculation: {CalculationTime}ms");
        Console.WriteLine($"  >  Output:      {OutputTime}ms");
        Console.WriteLine();
        Console.WriteLine($"Average insertion time: {AverageInsertionTime} ms");
        Console.WriteLine($"Average calculation time: {AverageCalculationTime} ms");
        Console.WriteLine($"Average output time: {AverageOutputTime} ms");
        Console.WriteLine();
    
        // Write data to file in json
        // Get the path, based on the date and the title
        string path = Path.Combine(@"C:\Users\joachimd\source\is\master-thesis", "performance-reports", $"{DateTime.Now:yyyy-MM-dd}", $"{title}-{DateTime.Now:HH-m-s}.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    
        await using var file = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(file, this);
    
        Console.WriteLine($"Performance data written to {path}");
        Console.WriteLine("-{}-{}-{}-{}-{}-{}-{}-{}-{}-");
    }
}