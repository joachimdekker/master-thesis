//using ExcelCompiler.Generated;
//using ExcelCompiler.GeneratedProjectRunner;
//using System.Diagnostics;
//using System.Numerics;

//internal class ServiceInvoice
//{
//    public static async Task Main(string[] args)
//    {
//        ExcelCompiler.Generated.Program program = new();

//        double total = 0;
//        string runId = Guid.NewGuid().ToString()[..8];


//        Console.WriteLine(Stopwatch.Frequency);
//        int count = 10_000_000;
//        const string workbookPath = @"C:\Users\jdekk\source\uva\master-thesis\spreadsheets\Service invoice.xlsx";
        
//        var outputDefintiion = new
//        {
//            Sheet = "Service invoice",
//            Cell = "E29"
//        };


//        // Generate input
//        Console.WriteLine("Generating inputs...");
//        Stopwatch inputSw = Stopwatch.StartNew();
//        int seed = 42;
//        Random random = new(seed);
//        List<Inputs> inputs = Enumerable.Range(0, count).Select(_ => Inputs.Create(random)).ToList();
//        inputSw.Stop();
//        Console.WriteLine($"Generated inputs in {inputSw.Elapsed.TotalMilliseconds} ms");


//        Console.WriteLine("Starting Excel calculation test...");
//        List<double> outputs = new();

//        List<long> insertionTimes = new();
//        List<long> calculationTimes = new();
//        List<long> extractionTimes = new();

//        Stopwatch wallTime = Stopwatch.StartNew();
//        Stopwatch insertionTime = new();
//        Stopwatch calculationTime = new();
//        Stopwatch extractionTime = new();

//        // Excelerate
//        for (int i = 0; i < count; i++)
//        {
//            var input = inputs[i];
//            calculationTime.Restart();
//            var output = program.Main(input.SalesTax, input.InvoiceDetails);
//            calculationTime.Stop();

//            insertionTimes.Add(0);
//            calculationTimes.Add(calculationTime.ElapsedTicks);
//            extractionTimes.Add(0);

//            outputs.Add(output);
    
//            if ((i + 1) % 5000 == 0) // Print less often, or every time if you want
//                DrawProgressBar(i + 1, count);
//        }
//        wallTime.Stop();

//        PerformanceData compiledPerformanceData = new(runId, workbookPath, wallTime.ElapsedTicks, insertionTimes, calculationTimes, extractionTimes);

//        await compiledPerformanceData.Report("Excelerate");


//        // Excel Interop
//        var excel = new Microsoft.Office.Interop.Excel.Application
//        { 
//            Visible = false,
//            ScreenUpdating = false,
//            DisplayAlerts = false,
//            EnableEvents = false,
//            Interactive = false,
//        };
//        var wb    = excel.Workbooks.Open(workbookPath, ReadOnly: false);

//        excel.CalculateBeforeSave = true;
//        excel.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;
        
//        List<double> excelOutputs = new();

//        wallTime.Restart();
//        calculationTimes.Clear();
//        insertionTimes.Clear();
//        extractionTimes.Clear();
//        calculationTime.Reset();

//        for (int i = 0; i < count; i++)
//        {
//            var inp = inputs[i];
//            PutInputIntoSpreadsheet(inp, wb, insertionTime);
    
//            // Do the real calculation
//            calculationTime.Restart();
//            excel.Calculate();
//            calculationTime.Stop();

//            // Grab output
//            extractionTime.Restart();
//            double outVal = Convert.ToDouble(wb.Sheets[outputDefintiion.Sheet].Range[outputDefintiion.Cell].Value2);
//            extractionTime.Stop();
    
//            calculationTimes.Add(calculationTime.ElapsedTicks);
//            insertionTimes.Add(insertionTime.ElapsedTicks);
//            extractionTimes.Add(extractionTime.ElapsedTicks);
//            excelOutputs.Add(outVal);
    
//            if ((i + 1) % 100 == 0) // Print less often, or every time if you want
//                DrawProgressBar(i + 1, count);
//        }
//        wallTime.Stop();

//        wb.Close(false);
//        excel.Quit();

//        PerformanceData excelPerformanceData = new(runId, workbookPath, wallTime.ElapsedTicks, insertionTimes, calculationTimes, extractionTimes);

//        await excelPerformanceData.Report("Excel");

//        // Check if every output is the same
//        double tolerance = 0;
//        bool allEqual = true;
//        for (int i = 0; i < count; i++)
//        {
//            if (Math.Abs(excelOutputs[i] - outputs[i]) > tolerance)
//            {
//                allEqual = false;
//                Console.WriteLine($"Output mismatch at index {i}: {excelOutputs[i]} != {outputs[i]}");
//            }
//        }

//        if (allEqual) Console.WriteLine("All outputs are equal!");

//        // Comparisons
//        Console.WriteLine("\n\nComparing performance data...");
//        Console.WriteLine($"Speedup in Wall Time: {excelPerformanceData.WallTime / compiledPerformanceData.WallTime}x");
//        Console.WriteLine($"Speedup in Calc Time: {excelPerformanceData.CalculationTime.Mean / compiledPerformanceData.CalculationTime.Mean}x");


//        void PutInputIntoSpreadsheet(Inputs inp, Microsoft.Office.Interop.Excel.Workbook wb, Stopwatch sw)
//        {
//            object[,] qty = new object[inp.InvoiceDetails.Count, 1];
//            object[,] price = new object[inp.InvoiceDetails.Count, 1];
//            for (int i = 0; i < inp.InvoiceDetails.Count; i++)
//            {
//                qty[i, 0] = inp.InvoiceDetails[i].Qty;
//                price[i, 0] = inp.InvoiceDetails[i].UnitPrice;
//            }
            
//            // Write to spreadsheet
//            sw.Restart();
//            wb.Sheets["Service invoice"].Range["B17:B25"].Value2 = qty;
//            wb.Sheets["Service invoice"].Range["D17:D25"].Value2 = price;
//            wb.Sheets["Service invoice"].Range["E28"].Value2 = (object)inp.SalesTax;
//            sw.Stop();
//        }

//        void DrawProgressBar(int progress, int total, int barSize = 80)
//        {
//            double percent = (double)(progress) / total;
//            int filled = (int)Math.Round(barSize * percent);
//            string bar = new string('█', filled) + new string(' ', barSize - filled);
//            string percentText = (percent * 100).ToString("0.0").PadLeft(5) + "%";
//            Console.Write($"\r[{bar}] {percentText} ({progress}/{total})");
//            if (progress == total)
//                Console.WriteLine("\n"); // Move to next line at the end
//        }
//    }
//}

//file class Inputs
//{
//    public double SalesTax { get; set; }
    
//    public List<InvoiceDetailsItem> InvoiceDetails { get; set; }

//    public static Inputs Create(Random random)
//    {
//        var lengths = new
//        {
//            Items = 9,
//        };
        
//        var invoiceDetails = Enumerable.Range(0, lengths.Items).Select(_ => new InvoiceDetailsItem(random.Next(1, 50), random.Next(100, 200))).ToList();
//        var salesTax = random.NextDouble(0.1, 0.2);
        
//        return new Inputs()
//        {
//            InvoiceDetails = invoiceDetails,
//            SalesTax = salesTax,
//        };
//    }
//}