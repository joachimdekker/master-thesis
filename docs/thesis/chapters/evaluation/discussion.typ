= Discussion<sec:eval:discussion>

In this section, we discuss the results described in @sec:eval:results. We also discuss the readability of the compiled code based on compiled snippets. 

- Discuss the semantic equality
  - We have complete equality. This could make Excelerate a substitute for Excel. However, we don't support that many operations yet.

- Discuss the speedups and what this means for the compiler
  - Reduce the discussed 2 weeks down to 5 minutes.
  - Way more robust and extendible.
  - Discuss the role of the JIT compiler, something that Excel does not have. C\# compiles to IL code, which is interpreted by the JIT compiler and converted on-demand to machine code @microsoft_managed_2025. This has a small overhead the first time we call a function.
  
- Discuss the slow insertion and extraciton speed and explain that this is no issue for Excelerate since we just use C\#.

- The performance increases and semantic equality can lead to the compiler being viable in business critical applications.

- Discuss the limited set of operations, but mention it can be extended.

== Readability and Idiomatic<sec:eval:readability>

- Structural Abstraction is way better in this new: common logic is bundled into classes, such as the interest logic. No more scattering of lots of variables. The domain models make the code more comprehensible by communicating these domain concepts instead of cell names. Leads to a reduction in cognitive load.
  - Using LINQ for operations on columns with Select and Sum make it clear that we are working on a table with that element and also name the element we are working on now.
  - The code footprint is now smaller.
- When not using parametrized structures, the creation of the structures takes up a large part of the 
- Both versions make use of idiomatic C\# such as LINQ calls (Select and Sum), the latest language features such as the target-typed new, 
- The newer version applies the DRY principles (@sec:intro:idiomatic-code). The repetitions in the calculation of a column in a table or a column in a chain is heavily simplified.
- The compiler produces some strange artifacts such as the ellipses in order to preserve the order of operations.
- The newer version makes integrating with the code a lot better.
- There is no use of whitespace to separate some structures. Plus There is no use of functions to separate functionality.

#figure(
  ```cs
    public double Main()
    {
        List<MonthlyBudgetReportC14F17Item> monthlyBudgetReportC14F17 = [new(6000, 5800), new(1000, 2300), new(2500, 1500)];
        double monthlyBudgetReportE9 = monthlyBudgetReportC14F17.Select(t => t.Actual).Sum();
        InterestC4F65 interestC4F65 = new([500, 500, ..., 500, 500]);
        double interestF65 = interestC4F65.TotalAt(60);
        double interestF5 = interestC4F65.TotalAt(0);
        double interestJ11 = interestC4F65.Deposit.Sum();
        double interestJ12 = interestF65 - (interestF5) - (interestJ11);
        List<TBL_MonthlyExpensesItem> tBL_MonthlyExpenses = [new(40, 40), new(0, 0), ..., new(0, 0), new(450, 450)];
        double monthlyBudgetReportE8 = tBL_MonthlyExpenses.Select(t => t.ActualCost).Sum();
        double monthlyBudgetReportE7 = monthlyBudgetReportE9 + interestJ12 - (monthlyBudgetReportE8);
        double monthlyBudgetReportD9 = monthlyBudgetReportC14F17.Select(t => t.Projected).Sum();
        double monthlyBudgetReportD8 = tBL_MonthlyExpenses.Select(t => t.ProjectedCost).Sum();
        double monthlyBudgetReportD7 = monthlyBudgetReportD9 + interestJ12 - (monthlyBudgetReportD8);
        double monthlyBudgetReportF7 = monthlyBudgetReportE7 - (monthlyBudgetReportD7);
        return monthlyBudgetReportF7;
    }
}
  ```,
  caption: []
)

#figure(
  ```cs
  public class InterestC4F65
{
    public List<double> Deposit { get; set; }
    public Dictionary<int,double> _totalAtMemoization { get; set; } = new Dictionary<int,double>();

    public double InterestAt(int counter) => 0.015 / (12) * (TotalAt(counter - (1)));
    public double TotalAt(int counter)
    {
        int key = counter;
        if (_totalAtMemoization.ContainsKey(key))
        {
            return _totalAtMemoization[key];
        }

        if (counter == 0)
        {
            return 10000;
        }

        double result = TotalAt(counter - (1)) + InterestAt(counter - (0)) + Deposit[counter - (1)];
        _totalAtMemoization.Add(key, result);
        return result;
    }

    public InterestC4F65(List<double> deposit)
    {
        Deposit = deposit;
    }
}
  ```
)

#figure(
  ```cs
  public double Main()
  {
      List<double> monthlyBudgetReportE9List = 
      [
        5800,
        2300,
        1500
      ]
      double monthlyBudgetReportE9 = monthlyBudgetReportE9List.Sum();
      double interestJ9 = 0.015 / (12);
      double interestD6 = interestJ9 * (10000);
      double interestF6 = 10000 + interestD6 + 500;
      double interestD7 = interestJ9 * (interestF6);
      double interestF7 = interestF6 + interestD7 + 500;
      double interestD8 = interestJ9 * (interestF7);
      double interestF8 = interestF7 + interestD8 + 500;
      double interestD9 = interestJ9 * (interestF8);
      double interestF9 = interestF8 + interestD9 + 500;
      ...
      double interestD63 = interestJ9 * (interestF62);
      double interestF63 = interestF62 + interestD63 + 500;
      double interestD64 = interestJ9 * (interestF63);
      double interestF64 = interestF63 + interestD64 + 500;
      double interestD65 = interestJ9 * (interestF64);
      double interestF65 = interestF64 + interestD65 + 500;
      List<double> interestJ11List =
      [
          0,
          500,
          500,
          ...
          500,
          500
      ];
      double interestJ11 = interestJ11List.Sum();
      double interestJ12 = interestF65 - (10000) - (interestJ11);
      List<double> monthlyBudgetReportE8List = 
      [
          40,
          0,
          ...
          0,
          450
      ];
      double monthlyBudgetReportE8 = monthlyBudgetReportE8List.Sum();
      double monthlyBudgetReportE7 = monthlyBudgetReportE9 + interestJ12 - (monthlyBudgetReportE8);
      List<double> monthlyBudgetReportD9List = [
        6000,
        1000,
        2500
      ];
      double monthlyBudgetReportD9 = monthlyBudgetReportD9List.Sum();
      List<double> monthlyBudgetReportD8List = 
      [ 
          40,
          0,
          ...
          0,
          450
      ];
      double monthlyBudgetReportD8 = monthlyBudgetReportD8List.Sum();
      double monthlyBudgetReportD7 = monthlyBudgetReportD9 + interestJ12 - (monthlyBudgetReportD8);
      double monthlyBudgetReportF7 = monthlyBudgetReportE7 - (monthlyBudgetReportD7);
      return monthlyBudgetReportF7;
  }
  ```
)

== Threats to validity

Although these experiments show clear semantic equality and improvements in performance, there are several factors that limit the generalisability of our findings. 

- Discuss the validity of the tests

- Discuss the threat to validity that the semantic equality was approached, but not proven. While we have generated a considerable amount of tests, we cannot say for sure that there is semantic equality. While semantic equality was observed between the spreadsheets, this does not imply the Excel compiler to be semantically equivalent as there will be edge cases where this may not be the case.

- Discuss the threat to validity of the COM interop, which, while it is fair to include the COM interface since we both call Excel from C\#, it does not directly compare C\# with the Excel calculation engine. While we do see differences in speed when the spreadsheet is of a different size, ultimately, we cannot compare this directly with the Excel calculation engine until we know the overhead the COM interface creates. It affects the speed-up ratio.

- The subjectivity of the readability definition is a threat to validity. While we do our best to explain the readability of the code, we (1) compare the code to compiled code we have written ourselves. (2) While we have two years of .NET experience, and follow the .NET guidelines to the letter, we may be judging with less experience and other programmers might find this bad code.

// This means comparing the methods in terms of speed is not done directly, but we rather compare the code compiled with Excelerate with Excel _plus_ the .NET COM interface overhead. This could be seen as a threat to validity. However, since we _have_ to work in C\#, it is fair to include the .COM interface overhead 