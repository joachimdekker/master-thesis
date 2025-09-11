// using System.Diagnostics;
// using System.Text.Json;
// using ExcelCompiler.GeneratedProjectRunner;
//
// internal class FamilyBudgetMonthly
// {
//     public static async Task Main(string[] args)
//     {
//         ExcelCompiler.Generated.Program program = new();
//
//         double total = 0;
//
//         int count = 10_000;
//         const string workbookPath = @"C:\Users\joachimd\source\is\master-thesis\spreadsheets\Family budget monthly.xlsx";
//         var outputDefintiion = new
//         {
//             Sheet = "Monthly Budget Report",
//             Cell = "F7"
//         };
//
//
// // Generate input
//         Console.WriteLine("Generating inputs...");
//         Stopwatch inputSw = Stopwatch.StartNew();
//         int seed = 42;
//         Random random = new(seed);
//         List<Inputs> inputs = Enumerable.Range(0, count).Select(i => new Inputs
//         {
//             MonthlyBudgetReportC14F17 = GenerateMonthlyBudgetReport(random, 3),
//             TBL_MonthlyExpenses = GenerateMonthlyExpenses(random, 59),
//             InterestC4F65 = GenerateInterest(random, 60)
//         }).ToList();
//         inputSw.Stop();
//         Console.WriteLine($"Generated inputs in {inputSw.Elapsed.TotalMilliseconds} ms");
//
//
//         Console.WriteLine("Starting Excel calculation test...");
//         List<double> outputs = new();
//         List<long> calculationTimes = new();
//
//         Stopwatch wallTime = Stopwatch.StartNew();
//         Stopwatch calculationTime = new();
//         Stopwatch individualCalculationTime = new();
//         for (int i = 0; i < count; i++)
//         {
//             var input = inputs[i];
//             calculationTime.Start();
//             individualCalculationTime.Restart();
//             var output = program.Main(input.MonthlyBudgetReportC14F17, input.TBL_MonthlyExpenses, input.InterestC4F65);
//             individualCalculationTime.Stop();
//             calculationTime.Stop();
//     
//             calculationTimes.Add(individualCalculationTime.ElapsedTicks);
//             outputs.Add(output);
//     
//             if (i % 100 == 0 || i == count - 1) // Print less often, or every time if you want
//                 DrawProgressBar(i + 1, count);
//         }
//         wallTime.Stop();
//
//         PerformanceData compiledPerformanceData = new()
//         {
//             WallTime = wallTime.Elapsed.TotalMilliseconds,
//             CalculationTime = calculationTime.Elapsed.TotalMilliseconds,
//             InsertionTime = 0,
//             OutputTime = 0,
//             CalculationTimes = calculationTimes,
//             Count = count,
//         };
//
//         await ReportPerformance("Excelerate", compiledPerformanceData);
//
//
// // Excel Interop
//         Stopwatch inputInjection = new();
//         Stopwatch outputExtraction = new();
//
//         var excel = new Microsoft.Office.Interop.Excel.Application
//         { 
//             Visible = false,
//             ScreenUpdating = false,
//             DisplayAlerts = false,
//             EnableEvents = false,
//             Interactive = false,
//         };
//         var wb    = excel.Workbooks.Open(workbookPath, ReadOnly: false);
//
//         excel.CalculateBeforeSave = true;
//         excel.Calculation = Microsoft.Office.Interop.Excel.XlCalculation.xlCalculationManual;
//
//         List<double> excelOutputs = new();
//
//         wallTime.Restart();
//         calculationTime.Reset();
//         calculationTimes.Clear();
//         individualCalculationTime.Reset();
//         for (int i = 0; i < count; i++)
//         {
//             inputInjection.Start();
//             var inp = inputs[i];
//             PutInputIntoSpreadsheet(inp, wb);
//             inputInjection.Stop();
//     
//             // Do the real calculation
//             calculationTime.Start();
//             individualCalculationTime.Restart();
//             excel.Calculate();
//             individualCalculationTime.Stop();
//             calculationTime.Stop();
//
//             // Grab output
//             outputExtraction.Start();
//             double outVal = Convert.ToDouble(wb.Sheets[outputDefintiion.Sheet].Range[outputDefintiion.Cell].Value2);
//             outputExtraction.Stop();
//     
//             calculationTimes.Add(individualCalculationTime.ElapsedTicks);
//             excelOutputs.Add(outVal);
//     
//             if (i % 100 == 0 || i == count - 1) // Print less often, or every time if you want
//                 DrawProgressBar(i + 1, count);
//         }
//         wallTime.Stop();
//
//         wb.Close(false);
//         excel.Quit();
//
//         PerformanceData excelPerformanceData = new()
//         {
//             WallTime = wallTime.Elapsed.TotalMilliseconds,
//             CalculationTime = calculationTime.Elapsed.TotalMilliseconds,
//             InsertionTime = inputInjection.Elapsed.TotalMilliseconds,
//             OutputTime = outputExtraction.Elapsed.TotalMilliseconds,
//             Count = count,
//             CalculationTimes = calculationTimes,
//         };
//
//         await ReportPerformance("Excel", excelPerformanceData);
//
// // Check if every output is the same
//         double tolerance = 1E-6;
//         bool allEqual = true;
//         for (int i = 0; i < count; i++)
//         {
//             if (Math.Abs(excelOutputs[i] - outputs[i]) > tolerance)
//             {
//                 allEqual = false;
//                 Console.WriteLine($"Output mismatch at index {i}: {excelOutputs[i]} != {outputs[i]}");
//             }
//         }
//
//         if (allEqual) Console.WriteLine("All outputs are equal!");
//
// // Comparisons
//         Console.WriteLine("\n\nComparing performance data...");
//         Console.WriteLine($"Speedup in Wall Time: {excelPerformanceData.WallTime / compiledPerformanceData.WallTime}x");
//         Console.WriteLine($"Speedup in Calc Time: {excelPerformanceData.CalculationTime / compiledPerformanceData.CalculationTime}x");
//
//
//         void PutInputIntoSpreadsheet(Inputs inp, Microsoft.Office.Interop.Excel.Workbook wb)
//         {
//             // Table 1
//             object[,] monthlyBudgetD14E17 = new object[inp.MonthlyBudgetReportC14F17.Count, 2];
//             for (int i = 0; i < inp.MonthlyBudgetReportC14F17.Count; i++)
//             {
//                 monthlyBudgetD14E17[i, 0] = inp.MonthlyBudgetReportC14F17[i].Projected;
//                 monthlyBudgetD14E17[i, 1] = inp.MonthlyBudgetReportC14F17[i].Actual;
//             }
//
//             wb.Sheets["Monthly budget report"].Range["D15:E17"].Value2 = monthlyBudgetD14E17;
//     
//             // Table 2
//             object[,] tBL_MonthlyExpensesD1E69 = new object[inp.TBL_MonthlyExpenses.Count, 2];
//             for (int i = 0; i < inp.TBL_MonthlyExpenses.Count; i++)
//             {
//                 tBL_MonthlyExpensesD1E69[i, 0] = inp.TBL_MonthlyExpenses[i].ProjectedCost;
//                 tBL_MonthlyExpensesD1E69[i, 1] = inp.TBL_MonthlyExpenses[i].ActualCost;
//             }
//     
//             wb.Sheets["Monthly expenses"].Range["E5:F63"].Value2 = tBL_MonthlyExpensesD1E69;
//     
//             // Interest
//             object[,] interestD6E65 = new object[inp.InterestC4F65.Deposit.Count, 1];
//             for (int i = 0; i < inp.InterestC4F65.Deposit.Count; i++)
//             {
//                 interestD6E65[i, 0] = inp.InterestC4F65.Deposit[i];
//             }
//             wb.Sheets["Interest"].Range["E6:E65"].Value2 = interestD6E65;
//         }
//
//         void DrawProgressBar(int progress, int total, int barSize = 100)
//         {
//             double percent = (double)(progress) / total;
//             int filled = (int)Math.Round(barSize * percent);
//             string bar = new string('█', filled) + new string(' ', barSize - filled);
//             string percentText = (percent * 100).ToString("0.0").PadLeft(5) + "%";
//             Console.Write($"\r[{bar}] {percentText} ({progress}/{total})");
//             if (progress == total)
//                 Console.WriteLine("\n"); // Move to next line at the end
//         }
//
//
//         InterestC4F65 GenerateInterest(Random random, int length)
//         {
//             var deposit = Enumerable.Range(0, length).Select(i => (double)random.Next(250,750)).ToList();
//             return new InterestC4F65(deposit);
//         }
//
//         async Task ReportPerformance(string title, PerformanceData data)
//         {
//             Console.WriteLine("--{  Performance Report  }--");
//             Console.WriteLine($"Title: {title}");
//             Console.WriteLine($"Count: {data.Count}");
//             Console.WriteLine($"Total time: {data.WallTime}ms");
//             Console.WriteLine($"  >  Insertion:   {data.InsertionTime}ms");
//             Console.WriteLine($"  >  Calculation: {data.CalculationTime}ms");
//             Console.WriteLine($"  >  Output:      {data.OutputTime}ms");
//             Console.WriteLine();
//             Console.WriteLine($"Average insertion time: {data.AverageInsertionTime} ms");
//             Console.WriteLine($"Average output time: {data.AverageOutputTime} ms");
//             Console.WriteLine($"Average calculation time: {data.AverageCalculationTime} ms");
//             Console.WriteLine();
//     
//             // Write data to file in json
//             // Get the path, based on the date and the title
//             string path = Path.Combine(@"C:\Users\joachimd\source\is\master-thesis", "performance-reports", $"{DateTime.Now:yyyy-MM-dd}", $"{title}-{DateTime.Now:HH-m-s}.json");
//             Directory.CreateDirectory(Path.GetDirectoryName(path)!);
//     
//             await using var file = File.OpenWrite(path);
//             await JsonSerializer.SerializeAsync(file, data);
//     
//             Console.WriteLine($"Performance data written to {path}");
//             Console.WriteLine("-{}-{}-{}-{}-{}-{}-{}-{}-{}-");
//         }
//
//         List<MonthlyBudgetReportC14F17Item> GenerateMonthlyBudgetReport(Random random, int length)
//         {
//             var actual = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//             var projected = Enumerable.Range(0, length).Select(i => (double)random.Next(1000, 2000)).ToList();
//             return Enumerable.Range(0, length).Select(i => new MonthlyBudgetReportC14F17Item(actual[i], projected[i])).ToList();
//         }
//
//         List<TBL_MonthlyExpensesItem> GenerateMonthlyExpenses(Random random, int length)
//         {
//             var actualCost = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//             var projectedCost = Enumerable.Range(0, length).Select(i => (double)random.NextDouble(1000, 2000)).ToList();
//             return Enumerable.Range(0, length).Select(i => new TBL_MonthlyExpensesItem(actualCost[i], projectedCost[i])).ToList();
//         }
//     }
// }
//
// file class Inputs
// {
//     public List<MonthlyBudgetReportC14F17Item> MonthlyBudgetReportC14F17 { get; set; }
//     public List<TBL_MonthlyExpensesItem> TBL_MonthlyExpenses { get; set; }
//     public InterestC4F65 InterestC4F65 { get; set; }
// }
//
// file record PerformanceData
// {
//     public required int Count { get; init; }
//     
//     public required double WallTime { get; init; }
//     public required double InsertionTime { get; init; }
//     public required double CalculationTime { get; init; }
//     public required double OutputTime { get; init; }
//     
//     public double AverageOutputTime => OutputTime / Count;
//     public double AverageCalculationTime => CalculationTime / Count;
//     public double AverageInsertionTime => InsertionTime / Count;
//     
//     public required List<long> CalculationTimes { get; set; }
// }