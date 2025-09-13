= Results<sec:eval:results>

For the results, we briefly discuss the results for Semantic Equality. Then, we dive deeper into the results of the performance aspect of the experiments, showing the differences between the two programs.

== Semantic Equality<subsec:eval:semantic-equality>

All spreadsheets passed this experiments. For every spreadsheet, for all $n = 100000$ experiments, every output comparison satisfied the defined equation, resulting in an Equality Rate of 1.0 across all spreadsheets.

== Performance

The Execution time was recorded for the compiled C\# code and the Excel evaluation for $n = 100000$ invocations per spreadsheet. Each component of the execution time was recorded separately for each invocation. The results can be seen in @table:results:performance.

#figure(
  table(
    columns: 5,
    table.vline(x: 1, start: 2, stroke: 3pt),
    table.header([*Spreadsheet*], table.cell([*Excel*], colspan: 3), [*Excelerate*]),
    [], [_Insertion_], [_Calculation_], [_Extraction_], [_Calculation_],
    table.hline(stroke: 3pt),
    [Family Monthly Budget], [$454308776 plus.minus 836570.2141$], [$4264.6897 plus.minus 2.677141$], [$3175.19929 plus.minus 2.39473$], [$5.12437 plus.minus 0.015240$],
    [Expense Tracker], [], [], [], [],
    [Service invoice], [$10687.40 plus.minus 8.8391$], [$2565.884 plus.minus 4.79967 $], [$4175.334 plus.minus 6.335414$], [$0.406945 plus.minus 0.042773$],
    [Retirement planner], [], [], [], [],
    [Actuarial Example], [], [], [], [],
  ),
  caption: [Overview of the experiment results that measure performance of Excelerate and Excel in nanoseconds (ns). Results are the average of $n=100000$. We also show the standard error.],
  placement: auto,
)<table:results:performance>

Excelerate manages to achieve sub-nanosecond performance on many spreadsheets, while Excel takes around 2 milliseconds for the calculations. There does not seem to be a lot of difference between the difference sheets.

The Excel calculations take considerably longer than Excelerate. The overhead of the COM interface is included in these figures. An average speedup of ... was observed.

An interesting observation can be made for the _warmup_ for the Excelerate compiler. The first run of the Excelerate compiler is often 250 times slower than the next run due to the overhead of the JIT compiler. We do not include this in the performance measures since it is only the first run and it significantly skews the variability.

// Should I say something about the variance of the data?

#import "@preview/cetz:0.4.0"
#import "@preview/cetz-plot:0.1.2": plot, chart

#figure({
  let data1 = (
    ([Excel], 0, 1068740.3582, 256588.4453, 417533.4169),
  )
  let data2 = (
    ([Excelerate], 85, 0, 511, 0),
  )
  align(left, [
  #cetz.canvas({
    chart.barchart(data1, 
      label-key: 0,
      value-key: (..range(1, 5)),
      size: (13, 2), mode: "stacked", labels: ([Compilation], [Insertion], [Calculation], [Extraction]), x-label: [Time (in ms)], bar-style: cetz.palette.new(colors: (orange, red, green, blue)),
      anchor: "bottom"
    )
  })
  #cetz.canvas({
   chart.barchart(data2,
     label-key: 0,
     value-key: (..range(1, 5)),
     size: (13, 2), mode: "stacked", x-label: [Time (in ms)], bar-style: cetz.palette.new(colors: (orange, red, green, blue))
   )
  })]
  )
  },
  caption: [A visualisation of the total amount of time it takes to execute 100000 invocations on the _Service Invoice_ spreadsheet. While _Compilation_ takes up more than 95% of the time, it only has to be run once.],
  placement: auto,
)

