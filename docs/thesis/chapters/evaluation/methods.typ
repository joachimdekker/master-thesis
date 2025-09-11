A compiler cannot be fully assessed on the design considerations alone. Its effectiveness must also be demonstrated through systematic evaluation. For the Excel Compiler, this involves the verification of the translation from spreadsheet formulas to C\#. The spreadsheet should produce the same values as its compiled C\# counterpart.

The proposed evaluation in this chapter serves two purposes: to approach the correctness of the compiler, and to assess the qualitative aspects of the generated code including performance, readability and adherence to idiomatic C\# practices. 

As such, this chapter reports on the evaluation along multiple factors. We first introduce the methods and metrics in @sec:eval:methods where we also touch upon what is considered idiomatic C\# code. Then, we show the results of the experiments in semantic equality and performance in @sec:eval:semantic-equality and @sec:eval:performance respectively. Furthermore, we discuss the readability of the code through several examples. Finally, we consolidate the findings in a discussion in @sec:eval:discussion, also denoting the threats to validity of the research and experiments.

= Methods<sec:eval:methods>

In this subsection, we propose several techniques to evaluate our Excel compiler. First, we discuss a way to analyze the semantics and performance of the compiled spreadsheet. Then, we discuss our method of assessing the readability of the generated code.

== Semantics and Performance
In our first experiment, we test the semantics and performance of Excelerate through a comparison test. Essentially, we generate random input for all eligible cells and compute the value from those inputs using both the Excel calculation engine, and our compiled spreadsheet code. We compare the inputs for equality and compare the performance of both methods. In the next subsections, we will discuss this more thoroughly.

=== Spreadsheet Selection
A corpora of spreadsheets is selected from the Excel Templates repository. This repository contains spreadsheets that are well-formatted and widely used.
Every spreadsheet is selected or adapted to support the compiler. Most spreadsheets contain error handling logic for invalid inputs, which are functions the compiler does not support, such as `IsError()`. 

Furthermore, we fix some dynamic aspects of a spreadsheet by removing the `Index` and similar functions. Functions like the `Index` function return a range and are mostly used for creating dynamic ranges that respond to user input. The compiler does not support this, and thus we replace this dynamic aspect by replacing these functions with a static range.

We selected 5 spreadsheets that represent different use-cases and sizes. Four spreadsheets are found in the Excel Templates repository, and one spreadsheet is taken directly from an actuarial context. // Some extra context? // Perhaps a appendix with changes?

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

=== Comparing the methods

We compare the methods along two axes: 
+ semantic equality through the calculated outputs and 
+ performance on several components through timers in the evaluation script.

#let Excelerate = smallcaps[Excelerate]

==== Semantic Equality
The semantic equality is validated by comparing the outputs of both evaluators. For every Trial Result object, the output of both evaluators should be the same, while accounting for floating point imprecisions. To test this, we use the following formula: $ abs(x_Excelerate - x_"Excel") <= epsilon $ where $epsilon$ is a very small value that accounts for floating point imprecisions.

The semantic equality of Excelerate is measured as an _Equality Rate_ (ER) $ "ER" = c / n $ which is expressed as a fraction of the number of correctly matching outputs $c$ over the total number of comparisons $n$.

==== Performance
Furthermore, the performance is recorded on several components. Through our preliminary tests, we noticed that the inserting and extraction of data in Excel took longer than anticipated. Hence, we also recorded this component resulting in the following components being measured:  $ t_"wall" = t_i + t_c + t_e $

where
  - $t_"wall"$ is the actual time it takes to complete the experiment.
  - $t_i$ is the _Injection Time_: The time it takes to inject the values into the Spreadsheet, measured by the time it takes to transform the values into the desired object and assigning them to the _Interop_ Excel worksheet object.
  - $t_c$ is the _Calculation Time_: The time it takes for Excel to finish calculating the dirty cells in the whole spreadsheet.
  - $t_e$ is the _Extraction Time_: The time it takes to extract the values out of the spreadsheet, measured by the time it takes to read the output cell property of the _Interop_ Excel worksheet object.

Durations are measured in nanoseconds using the .NET Diagnostics `StopWatch` class, which uses a high-resolution performance counter @microsoft_stopwatch_2025. However, due to the long nature of the experiments, we present the duration in milliseconds. 

=== Setup

All experiments have been done on a Dell Latitude Laptop with an Intel Core i7-12600VH VPRO and 32GB DDR4 RAM. The system was set to high-performance mode and was charging at all times.

==== Variability

In order to assess variability, we ran repeated experiments. When running repeated experiments, we used a seeded random number generator to ensure the same inputs were always used. The seed that was used in the following results was `42`.

To capture the variability of the results, we report the mean, the sample standard deviation (with Bessel's correction), and the standard error of the mean (SEM). For a series of samples: $(x_1, x_2, ..., x_n)$ we calculate the following:

$ overline(X) = 1 / (n) sum_(i=1)^n x_i $

$ sigma = sqrt(1/(n-1) sum^n_(i=1) (x_i - overline(X))^2) $

$ "SEM" = sigma / sqrt(n) $


// == Semantic Equality

// A big part of the effort is the validation of semantic equality: does the Excel spreadsheet calculate the same thing as the compiled version. We test this empirically, essentially unit testing the spreadsheets and compiled code. While this could also tests the performance of the compiler and compiled program, this method is primarily used for testing the semantic equality.

// Our method randomly selects an input and output cell. Then, it provides random values to the input cell and calculates the value in the output cell using two methods: the compiled spreadsheet and the spreadsheet loaded in Excel. If the results do not compare, we cannot have semantic equality.

// === Selection and preparation
// We provide a corpus of spreadsheets, containing spreadsheets found in a Template Directory provided by Microsoft. These spreadsheets were chosen to represent the different features of the compiler, and have been manually checked to only contain functions the compiler supports.

// For every workbook, we select random pairs in the spreadsheets. For instance, we might choose cell `B11` from the `Interest` sheet as input and `E10` from the `Monthly budget report` as output. We call this random pair a _Trial_. This is done in a uniform distribution out of a list of eligible cells. Eligible cells include cells that have numeric values or compute numeric values. For every workbook, we gather a predetermined number of _Trials_ to select. The randomness of the algorithm is seeded and persisted, meaning we get the same results every time.

// For every _Trial_, we want to run it several times to ensure that it works for multiple values. We assign a random number to every _Trial_ several times. One assignment is persisted as a _Trial Run_. For instance, the _Trial_ in the paragraph above might get three _Trial Runs_, and thus three numbers (100, 200, 300) assigned to the `B11` input. This would result in three _Trial Runs_.

// === Running and comparing

// In order to compare the results, we need to have two evaluators: the compiled program and the spreadsheet in Excel. The compiled program can be obtained by running the compiler on a spreadsheet to get the compiled program with the defined inputs and outputs in a _Trial_. If the compilation fails, the whole trial fails and all associated trials runs as well. Then, in order to get the results, we call the compiled program with the input values defined in a _Trial Run_.

// Getting values out of the spreadsheet is more complex. We utilize a part of the Microsoft Office Suite called the _Interop_. This enables us to alter a spreadsheet using C\# and evaluate formulas using the original calculation engine of Excel. For every _Trial_, we load the spreadsheet and load the values of a _Trial Run_ into the spreadsheet to calculate the values in the output as defined by the _Trial Run_.

// For every _Trial Run_, two components are run. First, the _Trial Run_ is evaluated against a compiled version of the spreadsheet. This result is stored as an incomplete _Trial Run Result_. To complete this object, we also evaluate the _Trial Run_ against the spreadsheet in Excel. The results are appended to the already existing _Trial Run Result_ for this _Trial Run_.

// In the end, the _Trial Run Result_ object is checked for equality. The output of both components should be the same, while accounting for floating point imprecisions. To test this, we use the following formula:

// $abs(x_"Compiled" - x_"Excel") <= epsilon$

// where $epsilon$ is a very small value that accounts for floating point imprecisions.

// === Metrics

// There are several metrics that are collected in this experiment. We briefly cover the metrics here and explain their importance for the compiler. 

// ==== Equality Rate
// The most important metric is the _Equality Rate_ or _Accuracy_ of the compiler. This is measured as the percentage of outputs that match between Excel and the compiled code. We also obtain this metric for every spreadsheet, in order to compare the equality rate between spreadsheets. If one spreadsheet has a low equality rate, it could be because the compiler does not support a certain edge case.

// A bonus metric that derives from the equality rate is the _Discrepancy Count_, which is the number of mismatches observed. As such, it is inversely related to the equality rate.

// ==== Execution Time
// The _Execution Time_ measures the time in milliseconds it takes to obtain the results when providing a program with input to output. We discuss this further in @subsec:eval:speed-comparisons. Within this experiment, we obtain this metric two times: once for the compiled program and one for Excel. We do not measure the execution time of one single iteration. Instead, we measure the time it takes for all _Trial Runs_ to complete for a certain _Trial_ and then divide this by the amount of _Trial Runs_.

// == Speed Comparisons<subsec:eval:speed-comparisons>
// Another essential part of the evaluation is the speedup observed when running the compiled code. Obviously, there would not be many advantages of using the compiler if the compiled code would be slower. As such, this experiment measures both methods---compiled code or Excel spreadsheet---in multiple ways. In this subsection, we first discuss what we are measuring. Then, we cover how we did this for both methods.

// === Method

// We measure the speed of both methods in several ways. We identified two stages that the compiled program would improve upon: the input and output, and the computation itself. As such, we test these hypotheses with several experiments. A big difference with the previous semantic equality experiment is the selection of the input and output cells. While the previous experiment chose the input and output randomly, this experiment carefully chooses the input and output cells in order to get the most diverse set of inputs. For instance, we always take the longest path through the spreadsheet as an input/output pair. 

// ==== Single Input
// This part purely measures the speed of the computation itself. We run the compiled code and Excel spreadsheet several times with random inputs. 'Running' the Excel Spreadsheet in this case means inserting the input value in the spreadsheet and letting Excel recalculate the output spreadsheet. This is done for 100, 1000, 10000, 100000, and 1000000 iterations. 

// ==== Structure Input
// One of the features of the compiler is the direct support of structure input. Instead of defining the individual cells as input, we can directly input the data for the structure itself. This part of the experiment measures the speed of the input and output. We generate random structure data and  Just like the single input, we measure this in iterations, in increasing numbers.

// An important limitation is that the length of the structure (i.e. the amount of rows in a table) should be the same as the original structure in the spreadsheet. For example, if the `Montly Expenses` table contains 20 rows, we must have 20 rows, not any more or less due to this limitation. This is because the range references in Excel are not dynamic and the table will not expand automatically to include extra rows (or inversely shrink and exclude rows).

// === Implementation

// In order to test the two methods (compiled code and Excel) we provide two implementations to test them. Abstractly, they are the same: We choose an input from a predefined list, we run the method and measure the iteration time. We do this for a predefined amount of iterations. Next, we briefly cover the details of how we got this to work on both the compiled code and excel.

// ==== Compiled Code
// For the compiled code, we take a spreadsheet and compile it to a class library. In the test project, we reference this class library. The input and output of the compiled code is elementary as it is just C\# objects and does not require further processing. We provide the input to the entry point of the library and measure the time it takes to complete.

// ==== Excel
// Just like the semantic equality experiment, Excel is more difficult to work with than the compiled code. Again, we utilize the Microsoft Office _Interop_ library that enables us to evaluate the spreadsheet using the Excel engine. The input is placed inside the the spreadsheet and we ask Excel to recalculate the cells. This yields the output.

// === Metrics
// There are several metrics that are collected in this experiment. We briefly cover the metrics here and explain their importance for the compiler. 

// ==== Execution Time
// The _Execution Time_ measures the time in milliseconds it takes to obtain the results when providing a program with input to output. The Execution Time is measured across all iterations. As such, we obtain the mean and standard deviation of the Execution Time. 

// #set enum(indent: 1em)

// In this experiment, we split the Execution Time into three stages:
// 1. Input Injection,
// 2. Calculation,
// 3. Output Retrieval.

// This allows us to accurately measure the time it takes to retrieve values and compare all aspects that contribute to the total execution time.

== Readability

- Describe the methods used to test the Excel Compiler
- Describe the metrics used in testing, such as seconds and the qualitative examples.
- Describe what is considered as idiomatic C\# code.