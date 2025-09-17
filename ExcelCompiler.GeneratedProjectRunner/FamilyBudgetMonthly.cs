//using System.Diagnostics;
//using System.Text.Json;
//using ExcelCompiler.Generated;
//using ExcelCompiler.GeneratedProjectRunner;

//internal class FamilyBudgetMonthly
//{
//    public static async Task Main(string[] args)
//    {
//        ExcelCompiler.Generated.Program program = new();


//        int count = 100_000;
//        const string workbookPath = @"C:\Users\jdekk\source\uva\master-thesis\spreadsheets\Family budget monthly.xlsx";
//        var outputDefintiion = new
//        {
//            Sheet = "Monthly Budget Report",
//            Cell = "F7"
//        };
//        string runId = Guid.NewGuid().ToString()[..8];

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
//            var output = program.Main(input.MonthlyBudgetReportC14F17, input.TBL_MonthlyExpenses, input.InterestC4F65);
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
//            // Table 1
//            object[,] monthlyBudgetD14E17 = new object[inp.MonthlyBudgetReportC14F17.Count, 2];
//            for (int i = 0; i < inp.MonthlyBudgetReportC14F17.Count; i++)
//            {
//                monthlyBudgetD14E17[i, 0] = inp.MonthlyBudgetReportC14F17[i].Projected;
//                monthlyBudgetD14E17[i, 1] = inp.MonthlyBudgetReportC14F17[i].Actual;
//            }


//            // Table 2
//            object[,] tBL_MonthlyExpensesD1E69 = new object[inp.TBL_MonthlyExpenses.Count, 2];
//            for (int i = 0; i < inp.TBL_MonthlyExpenses.Count; i++)
//            {
//                tBL_MonthlyExpensesD1E69[i, 0] = inp.TBL_MonthlyExpenses[i].ProjectedCost;
//                tBL_MonthlyExpensesD1E69[i, 1] = inp.TBL_MonthlyExpenses[i].ActualCost;
//            }


//            // Interest
//            object[,] interestD6E65 = new object[inp.InterestC4F65.Deposit.Count, 1];
//            for (int i = 0; i < inp.InterestC4F65.Deposit.Count; i++)
//            {
//                interestD6E65[i, 0] = inp.InterestC4F65.Deposit[i];
//            }

//            sw.Start();
//            wb.Sheets["Monthly budget report"].Range["D15:E17"].Value2 = monthlyBudgetD14E17; // 'Monthly budget report'!D15:E17
//            wb.Sheets["Monthly expenses"].Range["E5:F63"].Value2 = tBL_MonthlyExpensesD1E69; // 'Monthly expenses'!E5:F63
//            wb.Sheets["Interest"].Range["E6:E65"].Value2 = interestD6E65; // 'Interest'!E6:E65

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
//    public List<MonthlyBudgetReportC14F17Item> MonthlyBudgetReportC14F17 { get; set; }
//    public List<TBL_MonthlyExpensesItem> TBL_MonthlyExpenses { get; set; }
//    public InterestC4F65 InterestC4F65 { get; set; }

//    public static Inputs Create(Random random)
//    {
//        return new Inputs
//        {
//            MonthlyBudgetReportC14F17 = GenerateMonthlyBudgetReport(random, 3),
//            TBL_MonthlyExpenses = GenerateMonthlyExpenses(random, 59),
//            InterestC4F65 = GenerateInterest(random, 60)
//        };

//        List<MonthlyBudgetReportC14F17Item> GenerateMonthlyBudgetReport(Random random, int length)
//        {
//            var actual = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//            var projected = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//            return Enumerable.Range(0, length).Select(i => new MonthlyBudgetReportC14F17Item(actual[i], projected[i])).ToList();
//        }
//        List<TBL_MonthlyExpensesItem> GenerateMonthlyExpenses(Random random, int length)
//        {
//            var actualCost = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//            var projectedCost = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//            return Enumerable.Range(0, length).Select(i => new TBL_MonthlyExpensesItem(actualCost[i], projectedCost[i])).ToList();
//        }
//        InterestC4F65 GenerateInterest(Random random, int length)
//        {
//            var deposit = Enumerable.Range(0, length).Select(i => (double)random.Next(250, 750)).ToList();
//            return new InterestC4F65(deposit);
//        }
//    }
//}