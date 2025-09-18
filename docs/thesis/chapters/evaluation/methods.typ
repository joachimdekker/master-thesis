A compiler cannot be fully assessed on its design considerations alone. Its effectiveness must also be demonstrated through systematic evaluation. For Excelerate, this involves the verification of the translation from spreadsheet formulas to C\#. The spreadsheet should produce the same values as its compiled C\# counterpart.

The proposed evaluation in this chapter serves two purposes: to test the correctness of the compiler, and to assess the qualitative aspects of the generated code including performance, readability and adherence to idiomatic C\# practices. 

As such, this chapter reports on the evaluation along multiple factors. We first introduce the methods and metrics in @sec:eval:methods where we also touch upon what is considered idiomatic C\# code. Then, we show the results of the experiments in semantic equality and performance in @sec:eval:results. Furthermore, we discuss the readability of the code through several examples. Finally, we consolidate the findings in a discussion in @sec:eval:discussion, also denoting the threats to validity of the research and experiments.

= Methods<sec:eval:methods>

In this subsection, we propose several techniques to evaluate our Excel compiler. First, we discuss a way to analyze the semantics and performance of the compiled spreadsheet. Then, we discuss our method of assessing the readability of the generated code.

== Semantics and Performance
In our first experiment, we test the semantics and performance of Excelerate through a comparison test. Essentially, we generate random input for all eligible cells and compute the value from those inputs using both the Excel calculation engine, and our compiled spreadsheet code. We compare the inputs for equality and compare the performance of both methods. In the next subsections, we will discuss this more thoroughly.

=== Spreadsheet Selection
We select the spreadsheets from the Microsoft Create Excel Templates repository. This repository contains spreadsheets that are well-formatted and widely used. We select spreadsheet based on the compatibility with the compiler, since most spreadsheets contain formulas too complex for the compiler. Most spreadsheets contain error handling logic for invalid inputs, which are functions the compiler does not support, such as `IsError()`. The selected spreadsheets do not contain these functions, or we remove them by adapting the spreadsheet to the compiler.

Furthermore, we fix some dynamic aspects of a spreadsheet by removing the `Index` and similar functions. Functions like the `Index` function return a range and are mostly used for creating dynamic ranges that respond to user input. The compiler does not support this, and thus we replace this dynamic aspect by replacing these functions with a static range.

We selected 5 spreadsheets that represent different use-cases and sizes. Four spreadsheets are found in the Excel Templates repository, and one spreadsheet is taken directly from an actuarial context. @table:results:insertions provides an overview of the spreadsheets.



#figure([
  #set text(size: 0.8em)
  #table(columns: 5,
  table.header([*Name*], [*Cells / CU*], [*Structures*], [*Inputs*], [*Description*]),
  [Monthly Budget],    [320 / 701], [2 Tables, 1 Chain], [3  (3 ranges)], [Computationally the most diverse workbook, containing tables describing incomes and expenses and a way to calculate interest on a savings account.],
  [Holiday Budget],    [83 / 85], [6 Tables], [6 ( 6 range)], [Describes the budget of a holiday with six tables.],
  [Service Invoice],    [30 / 53], [1 Table], [2 (1 range + 1 cell)], [A very simple workbook that describes an invoice with one table containing services purchased.],
  [Retirement Planner], [1049 / 5733], [1 Chain], [3 (1 range + 2 cell)], [A workbook describing the interest on a bank account with monthly withdrawals for pension planning.],
  [Actuarial Example],  [705 / 3007], [2 Tables], [2 (2 ranges)], [An example workbook containing actuarial calculations, provided by DNB (De Nederlandse Bank). This spreadsheet is more than 150Mb.],
  )
],
  caption: [An overview of the Spreadsheets used in the experiments. The table describes the name and gives a description of their contents. We also denote the amount of cells, compute units, structures and input to give a sense of the complexity of a spreadsheet.]
)<table:results:insertions>

#show "Trial": set text(style: "italic")
#show "Trial Result": set text(style: "italic")

=== Trial Creation

Not every cell can be an input. Hence, we manually determine what cells are inputs. Based on the limitations discussed in the previous chapter, we exclude cells that would break the compilation. For instance, references made to computed cells from structures cannot be an input. Furthermore, only numerical values are considered to be eligible.

Every workbook gets assigned one output cell that ensures that nearly all parts of a workbook is covered by the experiment. In other words, the calculations that compute the output cell preferably reference the whole workbook.

For every workbook, we consider all eligible cells as input. For every eligible cell, we compute a random value that is used. We call the collection of the output and all these random input values for a spreadsheet a Trial. For instance, a Trial for the 'Monthly budget report' spreadsheet includes random input values for the income, the expenses and the deposits.

The Trial is associated with a spreadsheet. This spreadsheet gets compiled with the eligible input cells and output as parameters. This creates a class library that is referenced and directly called by the evaluation script.

=== Running the Trials

To compare the methods we require evaluators for Excelerate and the spreadsheet in Excel. The Excelerate evaluator references the compiled class library and calls it directly with the generated input for any given Trial. The output is saved for future reference in a Trial Result object. There is no work needed to convert the 

Running the trials with the Excel calculation engine is more complex since we need the exact Excel application, not an external library. Hence, we utilize a part of the Microsoft Office Suite called the _Interop_. This enables us to alter a spreadsheet using C\# and evaluate formulas using the original calculation engine. Every spreadsheet is loaded using this method and the inputs of the Trial are copied into the spreadsheet. Using a C\# method provided by the _Interop_, we calculate the value in the output cell and append the Trial Result object with this value.

Using the _Interop_ comes at a cost since we need to transfer the data between the Excel application and the C\# evaluation code. This is done through the .NET COM interface, which involves moving data between processes. This introduces a non-deterministic overhead that is required to run Excel from C\#.

=== Comparing the methods

We compare the methods along two metrics: 
+ semantic equality through the calculated outputs and 
+ performance on several components through timers in the evaluation script.

#let Excelerate = smallcaps[Excelerate]

==== Semantic Equality
The semantic equality is validated by comparing the outputs of both evaluators. For every Trial Result object, the output of both evaluators should be the same, while accounting for floating point imprecisions. To test this, we use the following formula: $ abs(x_Excelerate - x_"Excel") <= epsilon $ where $epsilon = 10^(-6)$ is a very small value that accounts for floating point imprecisions.

The semantic equality of Excelerate is measured as an _Equality Rate_ (ER) $ "ER" = c / n $ which is expressed as a fraction of the number of correctly matching outputs $c$ over the total number of comparisons $n$.

==== Performance
Furthermore, the performance is recorded on several components. Through our preliminary tests, we noticed that the inserting and extraction of data in Excel took longer than anticipated. Hence, we also recorded this component resulting in the following components being measured:  $ t_"wall" = t_i + t_c + t_e $

where
  - $t_"wall"$ is the actual time it takes to complete the experiment.
  - $t_i$ is the _Injection Time_: The time it takes to inject the values into the Spreadsheet, measured by the time it takes to transform the values into the desired object and assigning them to the _Interop_ Excel worksheet object.
  - $t_c$ is the _Calculation Time_: The time it takes for Excel to finish calculating the dirty cells in the whole spreadsheet.
  - $t_e$ is the _Extraction Time_: The time it takes to extract the values out of the spreadsheet, measured by the time it takes to read the output cell property of the _Interop_ Excel worksheet object.

When measuring performance, we exclude outliers. These outliers are often caused by demanding background processes and are not representative samples for the methods. A point is an outlier when the value lies outside $1.5 times "IQR"$ from the closest quarter. This should decrease variability and produce samples that are representative of the methods. In order to assess variability, we ran repeated experiments. Every spreadsheet was evaluated 100 000 times with random input. Durations are measured using the .NET Diagnostics `Stopwatch` class, which uses a high-resolution performance counter to achieve sub-microsecond precision (100ns) @microsoft_stopwatch_2025 . However, due to the long nature of the experiments, we present the duration in milliseconds.

=== Setup

All experiments have been done on a desktop computer with a 13th Gen Intel(R) Core(TM) i7-13700 and 32GB DDR5 RAM. The system was set to high-performance mode and a program was running to prevent the computer from going into sleep mode.

==== Variability
To capture the variability of the results, we report the mean and the standard error. For a series of samples: $(x_1, x_2, ..., x_n)$ we calculate the following:

$ overline(X) = 1 / (n) sum_(i=1)^n x_i $

$ sigma = sqrt(1/(n-1) sum^n_(i=1) (x_i - overline(X))^2) $

$ "SE" = sigma / sqrt(n) $

== Readability

The second experiment assesses the readability of the code emitted by the Excel compiler. This is a subjective assessment, since judging the readability of code is hard to do objectively as we covered in @sec:intro:idiomatic-code. The main objective of this experiment is to showcase the improvements in readability between the 'basic' compiler and Excelerate. In the next sections, we briefly discuss how to obtain this code and what criteria we use for discussing the readability, based on @sec:intro:idiomatic-code. 

=== Preparation

Obtaining the code is done in two ways. We obtain the code for Excelerate by running the compiler for every spreadsheet, generating a project and storing the files. For the 'basic' compiler, we run the same compiler, but without the steps for structure detection as described in @sec:excelerate:compiler-changes. Essentially, we force that no structure is found by clearing the array of structures in the _Structure Model_ at the end of the _Structure Phase_. This forces Excelerate to fall back on the 'basic' compiler, compiling the code without structural guidance.

We use the same spreadsheets as in the previous experiment, as they represent a wide range of use-cases.

=== Criteria<par:readability:criteria>

Before we present the results and discuss the readability, we first reflect on the criteria of 'readable' code. As we covered in @sec:intro:idiomatic-code, we consider this highly subjective metric to be good if the code seems to be written by a experience developer in that particular language. This means that the code should be:
- _Expressive_ and _Comprehensible_: It should be very simple to figure out what the program does. This can be communicated through the use of variable names or smart use of language features such as LINQ.
- _Extensible_ and _Maintainable_: Since Excelerate is meant to be used as a class library, it should be easy to build programs on top of it. If a change needs to be made to the code, it needs to be easy to fix this. This also means that code should express a calculation only once and avoid code duplication.
- In accordance with the style guide: we use the Microsoft Style Guide for Csharp @microsoft_net_2025 for this. All Csharp code should adhere to this style guide. For instance, it states naming conventions for local variables, classes, properties, etc; which language constructs we should use and more.


// - Describe the methods used to test the Excel Compiler
// - Describe the metrics used in testing, such as seconds and the qualitative examples.
// - Describe what is considered as idiomatic C\# code.

