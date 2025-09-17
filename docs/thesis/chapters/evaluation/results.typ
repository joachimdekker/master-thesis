= Results<sec:eval:results>

For the results, we briefly discuss the results for Semantic Equality. Then, we dive deeper into the results of the performance aspect of the experiments, showing the differences between the two programs.

== Semantic Equality<subsec:eval:semantic-equality>

All spreadsheets passed this experiments. For every spreadsheet, for all $n = 100000$ experiments, every output comparison satisfied the defined equation, resulting in an Equality Rate of 1.0 across all spreadsheets. A large fraction (>99%) of the results were bit-wise identical. The few differing results could still be accounted to floating point imprecision.

== Performance

The Execution time was recorded for the compiled C\# code and the Excel evaluation for $n = 100000$ invocations per spreadsheet. Each component of the execution time was recorded separately for each invocation. The results can be seen in @table:results:performance.

#[
  #show figure: it => {
    show table: t => {
      set text(size: 0.8em)
      t
    }
    it  
  }
  
  #figure(
    table(
      columns: 5,
      table.vline(x: 1, start: 2, stroke: 3pt),
      table.header([*Spreadsheet*], table.cell([*Excel*], colspan: 3), [*Excelerate*]),
      [], [_Insertion_], [_Calculation_], [_Extraction_], [_Calculation_],
      table.hline(stroke: 3pt),
      [Family Monthly Budget], [$454308776 plus.minus 836570.2141$], [$4264.6897 plus.minus 2.677141$], [$3175.19929 plus.minus 2.39473$], [$5.12437 plus.minus 0.015240$],
      [Expense Tracker], [$1028090647 plus.minus 1928430$], [$2324.3398 plus.minus 3.2663$], [$3696.03671 plus.minus 3.15537$], [$2.64764928 plus.minus 0.00375018$],
      [Service invoice], [$10687.40 plus.minus 8.8391$], [$2565.8843 plus.minus 4.79967 $], [$4175.334 plus.minus 6.335414$], [$0.406945 plus.minus 0.042773$],
      [Retirement planner], [], [], [], [],
      [Actuarial Example], [], [], [], [],
    ),
    caption: [Overview of the experiment results that measure performance of Excelerate and Excel in microseconds ($mu s$). Results are the average of $n=100000$. We also show the standard error.],
    placement: auto,
  )<table:results:performance>
]

Excelerate manages to achieve sub-nanosecond performance on many spreadsheets, while Excel takes around 2 milliseconds for the calculations. There does not seem to be a lot of difference between the difference sheets.

The Excel calculations take considerably longer than Excelerate. The overhead of the COM interface is included in these figures. An average speedup of ... was observed.

An interesting observation can be made for the _warmup_ for the Excelerate compiler. The first run of the Excelerate compiler is often 250 times slower than the next run due to the overhead of the JIT compiler. We do not include this in the performance measures since it is only the first run and it significantly skews the variability.

// Should I say something about the variance of the data?

#import "@preview/cetz:0.4.0"
#import "@preview/cetz-plot:0.1.2": plot, chart

#figure({
  let data1 = (
    ([Excel\ (1742862ms)], 0, 1068740.3582, 256588.4453, 417533.4169), // 1742862.2204
    ([Excelerate\ (126ms)], 85, 0, 41.1833, 0),
  )
  let data2 = (
    ([Excelerate\ (126ms)], 85, 0, 41.1833, 0),
  )
  align(left, [
  #cetz.canvas({
    chart.barchart(data1, 
      label-key: 0,
      value-key: (..range(1, 5)),
      size: (13, auto), mode: "stacked", labels: ([Compilation], [Insertion], [Calculation], [Extraction]), x-label: [Time (in ms)], bar-style: cetz.palette.new(colors: (orange, red, green, blue)),
      anchor: "bottom"
    )
  })
  #cetz.canvas({
   chart.barchart(data2,
     label-key: 0,
     value-key: (..range(1, 5)),
     size: (13, 2), mode: "stacked", x-label: [Time (in ms)], bar-style: cetz.palette.new(colors: (orange, red, green, blue))
   )
  })
]
  )
  },
  caption: [A visualisation of the total amount of time it takes to execute 100000 invocations on the _Service Invoice_ spreadsheet. The _Compilation_ step only has to be run once.],
  placement: auto,
)<table:results:overhead>

@table:results:overhead shows the amount of time it takes to execute 100 000 invocations of the smallest spreadsheet: _Service invoice_. Excelerate is considerably faster than Excel, finishing compilation and calculation in the same time that calculation is completed. The compilation is about 85ms, accounting for 66.7% of the total wall time. Conversely, if we look at the composition of the performance of Excel, the insertion of data into the spreadsheet takes the longest, accounting for 61.3% of the total wall time. The actual calculation only takes 14.7% of the total wall time.

The insertion times for _Family Monthly Budget_ and _Expense Tracker_ are significantly higher than the calculation and extraction combined. @table:results:insertions describes the amount of insertions needed. It seems that more insertions mean longer insertion times, which can be accounted to the COM overhead.

#figure(
  table(
    columns: 2,
    table.header([*Spreadsheet*], table.cell([*Insertions*])),
    [Family Monthly Budget], [3 (3 range)],
    [Expense Tracker], [6 ( 6 range)],
    [Service invoice], [2 (1 range + 1 cell)],
    [Retirement planner], [1 (1 cell)],
    [Actuarial Example], [3 (1 cell + 1 range)],
  ),
  caption: [Overview of the amount of insertions needed to provide Excel with the data for the experiments],
)<table:results:insertions>


