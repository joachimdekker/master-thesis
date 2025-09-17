//using ExcelCompiler.Generated;

//using System.Diagnostics;
//using System.Text.Json;
//using ExcelCompiler.GeneratedProjectRunner;

//internal static class FamilyBudgetMonthly
//{
//    public static async Task Main(string[] args)
//    {
//        ExcelCompiler.Generated.Program program = new();

//        double total = 0;

//        int count = 100_000;
//        const string workbookPath = @"C:\Users\jdekk\source\uva\master-thesis\spreadsheets\Holiday budget planner (Adapted).xlsx";
//        var outputDefintiion = new
//        {
//            Sheet = "Holiday budget planner",
//            Cell = "N6"
//        };
//        string runId = Guid.NewGuid().ToString()[..8];

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
//            var output = program.Main(input.Gifts, input.Meals, input.Packaging, input.Entertainment, input.Travel, input.Miscellaneous);
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
//        var wb = excel.Workbooks.Open(workbookPath, ReadOnly: false);

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
//            // Gifts
//            object[,] gifts = new object[inp.Gifts.Count, 2];
//            for (int i = 0; i < inp.Gifts.Count; i++)
//            {
//                gifts[i, 0] = inp.Gifts[i].Budget;
//                gifts[i, 1] = inp.Gifts[i].Actual;
//            }

//            // Meals
//            object[,] meals = new object[inp.Meals.Count, 2];
//            for (int i = 0; i < inp.Meals.Count; i++)
//            {
//                meals[i, 0] = inp.Meals[i].Budget;
//                meals[i, 1] = inp.Meals[i].Actual;
//            }

//            // Packaging
//            object[,] packaging = new object[inp.Packaging.Count, 2];
//            for (int i = 0; i < inp.Packaging.Count; i++)
//            {
//                packaging[i, 0] = inp.Packaging[i].Budget;
//                packaging[i, 1] = inp.Packaging[i].Actual;
//            }

//            // Entertainment
//            object[,] entertainment = new object[inp.Entertainment.Count, 2];
//            for (int i = 0; i < inp.Entertainment.Count; i++)
//            {
//                entertainment[i, 0] = inp.Entertainment[i].Budget;
//                entertainment[i, 1] = inp.Entertainment[i].Actual;
//            }

//            // Travel
//            object[,] travel = new object[inp.Travel.Count, 2];
//            for (int i = 0; i < inp.Travel.Count; i++)
//            {
//                travel[i, 0] = inp.Travel[i].Budget;
//                travel[i, 1] = inp.Travel[i].Actual;
//            }

//            // Miscellaneous
//            object[,] miscellaneous = new object[inp.Miscellaneous.Count, 2];
//            for (int i = 0; i < inp.Miscellaneous.Count; i++)
//            {
//                miscellaneous[i, 0] = inp.Miscellaneous[i].Budget;
//                miscellaneous[i, 1] = inp.Miscellaneous[i].Actual;
//            }

//            // Write to spreadsheet
//            sw.Start();
//            wb.Sheets["Holiday budget planner"].Range["D11:E16"].Value2 = gifts;
//            wb.Sheets["Holiday budget planner"].Range["L11:M16"].Value2 = meals;
//            wb.Sheets["Holiday budget planner"].Range["D21:E27"].Value2 = packaging;
//            wb.Sheets["Holiday budget planner"].Range["L21:M27"].Value2 = entertainment;
//            wb.Sheets["Holiday budget planner"].Range["D32:E35"].Value2 = travel;
//            wb.Sheets["Holiday budget planner"].Range["L32:M35"].Value2 = miscellaneous;
//            sw.Stop();
//        }

//        void DrawProgressBar(int progress, int total, int barSize = 100)
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
//    public List<EntertainmentItem> Entertainment { get; set; }

//    public List<GiftsItem> Gifts { get; set; }

//    public List<MealsItem> Meals { get; set; }

//    public List<PackagingItem> Packaging { get; set; }

//    public List<TravelItem> Travel { get; set; }

//    public List<MiscellaneousItem> Miscellaneous { get; set; }

//    public static Inputs Create(Random random)
//    {
//        var lengths = new
//        {
//            Meals = 6,
//            Gifts = 6,
//            Entertainment = 7,
//            Packaging = 7,
//            Miscellaneous = 4,
//            Travel = 4,
//        };

//        var entertainment = Enumerable.Range(0, lengths.Entertainment).Select(_ => new EntertainmentItem(random.Next(100, 200), random.Next(100, 200))).ToList();
//        var gifts = Enumerable.Range(0, lengths.Gifts).Select(_ => new GiftsItem(random.Next(100, 200), random.Next(100, 200))).ToList();
//        var meals = Enumerable.Range(0, lengths.Meals).Select(_ => new MealsItem(random.Next(100, 200), random.Next(100, 200))).ToList();
//        var packaging = Enumerable.Range(0, lengths.Packaging).Select(_ => new PackagingItem(random.Next(100, 200), random.Next(100, 200))).ToList();
//        var miscellaneous = Enumerable.Range(0, lengths.Miscellaneous).Select(_ => new MiscellaneousItem(random.Next(100, 200), random.Next(100, 200))).ToList();
//        var travel = Enumerable.Range(0, lengths.Travel).Select(_ => new TravelItem(random.Next(100, 200), random.Next(100, 200))).ToList();

//        return new Inputs()
//        {
//            Entertainment = entertainment,
//            Gifts = gifts,
//            Meals = meals,
//            Packaging = packaging,
//            Miscellaneous = miscellaneous,
//            Travel = travel,
//        };
//    }
//}