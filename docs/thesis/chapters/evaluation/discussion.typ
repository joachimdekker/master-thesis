#pagebreak(weak:true)
= Discussion<sec:eval:discussion>

In this section, we discuss the results described in @sec:eval:results. We also discuss the readability of the compiled code based on compiled snippets. 

The results show a reported total equality across all benchmark spreadsheets. This indicates that the compiled C\# code reproduce the exact behaviour of Excel's calculation engine. The findings directly answer RQ3, demonstrating that there is a way to verify semantic equivalence between Excel and generated code for the supported feature set. More precisely, the bit-wise equivalence found in many compared results indicates Excelerate often produces the same order of operations as Excel.

Our performance measurements indicate an average speedup of 500x. The main contributor to this speedup is the removal of the COM interface: Excelerate can be directly called from C\# code. However, this is not the only contribution. It can be seen that in larger spreadsheets such as _Family budget monthly_, the actual calculation time doubles in comparison with a smaller spreadsheet such as _Service Invoice_. The overhead of the COM interface should only interfere when data and commands are moving between program boundaries, not when the actual Excel sheet is calculating. This indicates that the Excel Calculation engine is also performing slower than Excelerates compiled code.

This can be explained by the optimised execution path. Excel Calculation engine is an interpreter and interpreted the cells. Conversely, the code Excelerate emits is further optimised by the .NET JIT compiler, significantly boosting performance @microsoft_managed_2025. Although the .NET compiler compiles and interpretes the code in IL, sections of the code are converted on-demand to machine code and directly run afterwards @microsoft_managed_2025. This gives a small overhead the first time we call a function for example.

The performance measurement revealed that instead of calculation, the insertion of data is the biggest slowing factor for Excel. Due to the large overhead of the COM interface, every time a outside program tries to insert a range of value, it gets this COM overhead, quickly accumulating when having multiple input ranges. Instead, Excelerate produces a class library with little to no input and output extraction overhead.

The increases in performance and evidence for semantic equality indicate Excelerate as a possible substitution for the use of Excel in business critical applications. Applying Excel to the example in the introduction, the two weeks it took to calculate the pension prediction for all 200 000 customers, would take approximately 5 minutes with Excelerate. That said, the limited operator and function set supported by Excelerate is a critical limiting factor. However, Excelerate can be extended to support more operators, functions and paradigms.

Furthermore, having compiled code instead of interpreted code with Excel does not only have performance advantages. Excelerate itself is language agnostic and can compile to many languages if extended. This allows the spreadsheet to also be compiled to code that can be run on device that do not have Excel installed.

// // The results show complete equality, meaning that any input can be calculated the same way in Excelerate at minimal precision loss. The fact that many floating point outputs are exactly the same indicates Excelerate often produces the same order of operations as Excel.

// - Discuss the semantic equality
//   - We have complete equality. This could make Excelerate a substitute for Excel. However, we don't support that many operations yet.

// - Discuss the speedups and what this means for the compiler
//   - Reduce the discussed 2 weeks down to 5 minutes.
//   - Way more robust and extendible.
//   - Discuss the role of the JIT compiler, something that Excel does not have. C\# compiles to IL code, which is interpreted by the JIT compiler and converted on-demand to machine code @microsoft_managed_2025. This has a small overhead the first time we call a function.
  
// - Discuss the slow insertion and extraciton speed and explain that this is no issue for Excelerate since we just use C\#.

// - The performance increases and semantic equality can lead to the compiler being viable in business critical applications.

// - Discuss the limited set of operations, but mention it can be extended.

== Readability and Idiomaticity<sec:eval:readability>

For this section, we use the code generated from the _Family Monthly Budget_ spreadsheet. This spreadsheet best represents the code improvements made by Excelerate over the 'basic' compiler. We present the new code in @code:discussion:fbm-new and the old code in @code:discussion:fbm-old. The code of some of the other spreadsheets can be found in the appendix.

=== Structural Abstraction

From a readability and comprehensibility standpoint, Excelerate code bundles common logic into classes that communicate their meaning directly. This removes the scattered variables that convey nothing about their semantics. 

The use of LINQ extends this. For instance, calls that compute the sum of a column with `Sum` and `Select` clearly state what part of an entity is being processed, and how it is being done. In comparison with @code:discussion:fbm-old, where it is just a `Sum` on a variable with no real meaning, this increases the comprehensibility of the code.

A side effect of this structural abstraction is significant less lines of codes and application of the DRY principle (see @sec:intro:idiomatic-code). In @code:discussion:fbm-new, the columns of the 'Monthly Expenses' now get merged into one construction instead of the two separate declarations in @code:discussion:fbm-old. Furthermore, the main repetition that calculates the values in the interest spreadsheet is abstracted in @code:discussion:fbm-new through a new class that recursively calculates. This improves maintainability and readability.

This new object-oriented design makes integration easier. Excelerate exposes groupings of variables by structure as input, allowing external code to easily populate these objects and call the generated code. Comparing this with the old code, which only supported individual variables, it 

=== Idiomaticity

Both versions of the code make use of idiomatic C\# principles such as the LINQ calls that enumerate lists instead of using a for-loop. They also utilize the latest language features such as the new collection-expression and the target-typed new, reducing the amount of redundant and repeated class names. Both are recommended by Microsoft as it improves readability by omiting the redundant types @microsoft_net_2025. Many variables names adhere to the style guide, with the exception of some of the structures in the new code: `TBL_MonthlyExpensesItem` is invalid as it uses underscores and wrong capitalisation. A better name would be `MonthlyExpensesItem`.

This directly complements the next point of critique: the variable names. While the variable names directly link back to the spreadsheet, allowing for traceability of the code back to the spreadsheet, they are not descriptive in any way. This is especially clear in @code:discussion:fbm-old, where without the context of the spreadsheet, it would be hard to determine the nature of the code.  This reduces the readability significantly.

Furthermore, the redundant parenthesis present in both versions reduces the readability of the code. Besides, the code does not follow the Microsoft Guidelines in the way the properties of the classes are structured: the guidelines state that `_totalAtMemoization` should be a readonly private field, instead of a public property @microsoft_net_2025. This may cause confusion amongst developers and thus reduces comprehension.

Finally, in both versions, the structure of the code can be improved by introducing whitespace. Both listings do not use empty lines to separate the structures, which could help structurally separating functionality. Furthermore, there is no use of functions to explicitly separate related functionality. For instance, the interest calculations in @code:discussion:fbm-new could have been in a function.

These shortcomings highlight that---while there are positive properties of the code---there is still room for improvement in making the code idiomatic. Based on the above facts, we do note that the code that was produced by Excelerate was more idiomatic than the code produced by the 'basic' compiler.

#import "@preview/zebraw:0.5.5": *
#show: zebraw.with(lang: false)

#figure([
  #zebraw(
    header: [Main.cs],
    // highlight-lines: (
    //   (1, [The Fibonacci sequence is defined through the recurrence relation $F_n = F_(n-1) + F_(n-2)$\
    //   It can also be expressed in _closed form:_ $ F_n = round(1 / sqrt(5) phi.alt^n), quad
    //   phi.alt = (1 + sqrt(5)) / 2 $]),
    //   // Passing a range of line numbers in the array should begin with `..`
    //   ..range(9, 14),
    //   (13, [The first \#count numbers of the sequence.]),
    // ),
    lang: false,
    block-width: 100%,
    wrap: false,
    
    text([```cs
    public double Main()
    {
        List<MonthlyBudgetReportC14F17Item> monthlyBudgetReportC14F17 = [new(6000, 5800), ..., new(2500, 1500)];
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
  ```], size: 0.7em),
  
)
 #zebraw(
   header: [InterestC4F65.cs],
    // highlight-lines: (
    //   (1, [The Fibonacci sequence is defined through the recurrence relation $F_n = F_(n-1) + F_(n-2)$\
    //   It can also be expressed in _closed form:_ $ F_n = round(1 / sqrt(5) phi.alt^n), quad
    //   phi.alt = (1 + sqrt(5)) / 2 $]),
    //   // Passing a range of line numbers in the array should begin with `..`
    //   ..range(9, 14),
    //   (13, [The first \#count numbers of the sequence.]),
    // ),
    lang: false,
    block-width: 100%,
    wrap: false,
    
    text([```cs
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
  ```], size: 0.7em),
  
)
],
  caption: []
)<code:discussion:fbm-new>

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
)<code:discussion:fbm-old>

== Threats to validity

Although these experiments show clear semantic equality and improvements in performance, there are several factors that limit the generalisability of our findings.

// First, the internal validity may be threatened by the warm-up effects cause by the .NET JIT compiler

The semantic equality experiment provided evidence of semantic equality, but it was not proven. We did not provide proof that Excelerate produces the exact same semantics due to the scope of the thesis, and instead opted to verify whether the semantic equality was present with a considerable amount of tests. While the results are promising, we cannot say for sure there is semantic equality. This is further threatened by the limited set of spreadsheets that we chose. While semantic equality was preserved between these spreadsheets, we cannot guarantee it works for all spreadsheets and all undiscovered edge cases.

Another threat to construct validity is the COM interop, which adds significance noise and variance to the results of the performance of Excel. It can be seen as fair to include the overhead of the COM interface in the results since Excel engine and Excelerate both have to be called from C\#. However, we cannot directly compare  while we do see differences in speed when the spreadsheet is of a different size, ultimately, we cannot compare this directly with the Excel calculation engine until we know the overhead the COM interface creates.

Readability is subjective, which means that the author with four years of C\# experience may have different standards than other, more experienced developers. We try to stick to the literature and the .NET guidelines. However, this subjectivity still introduces a threat to validity.

// While we do our best to explain the readability of the code, we (1) compare the code to compiled code we have written ourselves. (2) While we have two years of .NET experience, and follow the .NET guidelines to the letter, we may be judging with less experience and other programmers might find this bad code.

External validity is threatened by the limited set of simpler spreadsheets. Spreadsheets used in the real world are far more complex. Due to the supported set of features by the compiler, generalisation of the results to real-world spreadsheets is hard.

// - Discuss the threat to validity that the semantic equality was approached, but not proven. While we have generated a considerable amount of tests, we cannot say for sure that there is semantic equality. While semantic equality was observed between the spreadsheets, this does not imply the Excel compiler to be semantically equivalent as there will be edge cases where this may not be the case.

// - Discuss the threat to validity of the COM interop, which, while it is fair to include the COM interface since we both call Excel from C\#, it does not directly compare C\# with the Excel calculation engine. While we do see differences in speed when the spreadsheet is of a different size, ultimately, we cannot compare this directly with the Excel calculation engine until we know the overhead the COM interface creates. It affects the speed-up ratio.

// - The subjectivity of the readability definition is a threat to validity. While we do our best to explain the readability of the code, we (1) compare the code to compiled code we have written ourselves. (2) While we have two years of .NET experience, and follow the .NET guidelines to the letter, we may be judging with less experience and other programmers might find this bad code.

// This means comparing the methods in terms of speed is not done directly, but we rather compare the code compiled with Excelerate with Excel _plus_ the .NET COM interface overhead. This could be seen as a threat to validity. However, since we _have_ to work in C\#, it is fair to include the .COM interface overhead 