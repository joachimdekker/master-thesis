#import "@preview/cetz:0.4.0"
#import "@preview/cetz-plot:0.1.2": plot, chart

= Results<sec:eval:results>

For the results, we briefly discuss the results for Semantic Equality. Then, we dive deeper into the results of the performance aspect of the experiments, showing the differences between the two programs.

== Semantic Equality<subsec:eval:semantic-equality>

All spreadsheets passed this experiments. For every spreadsheet, for all $n = 100000$ experiments, every output comparison satisfied the defined equation with $epsilon = 10^(-6)$, resulting in an Equality Rate of 1.0 across all spreadsheets. A large fraction (>99%) of the results were bit-wise identical. The few differing results could still be accounted to floating point imprecision.

== Performance

The Execution time was recorded for the compiled C\# code and the Excel evaluation for $n = 100000$ invocations per spreadsheet. Each component of the execution time was recorded separately for each invocation. The results can be seen in @table:results:performance.

#[
  #show figure: it => {
    show table: t => {
      set text(size: 0.65em)
      t
    }
    it
  }
  
  #figure(
    table(
      columns: 6,
      table.vline(x: 1, start: 2, stroke: 3pt),
      table.header([*Spreadsheet*], table.cell([*Excel*], colspan: 3), [*Excelerate*], [*Speedup*]),
      [], [_Insertion_], [_Calculation_], [_Extraction_], [_Calculation_], [],
      table.hline(stroke: 3pt),
      [Monthly Budget], [$9.07 times 10^3 plus.minus 6.75 times 10^0$], [$4.26 times 10^3 plus.minus 2.68 times 10^0$], [$3.18 times 10^3 plus.minus 2.39 times 10^0$], [$5.12 times 10^0 plus.minus 1.52 times 10^(-2)$], [$8.32 times 10^2$],
      [Holiday Budget], [$2.07 times 10^4 plus.minus 1.33 times 10^1$], [$2.32 times 10^3 plus.minus 3.27 times 10^0$], [$3.70 times 10^3 plus.minus 3.16 times 10^0$], [$2.65 times 10^0 plus.minus 3.75 times 10^(-3)$], [$8.75 times 10^2$],
      [Service invoice], [$1.07 times 10^4 plus.minus 8.84 times 10^0$], [$2.57 times 10^3 plus.minus 4.80 times 10^0$], [$4.18 times 10^3 plus.minus 6.34 times 10^0$], [$4.07 times 10^(-1) plus.minus 4.28 times 10^(-2)$], [$6.32 times 10^3$],
      [Retirement planner], [$1.01 times 10^4 plus.minus 8.36 times 10^0$], [$1.26 times 10^4 plus.minus 4.43 times 10^0$], [$3.25 times 10^3 plus.minus 1.92 times 10^0$], [$2.25 times 10^1 plus.minus 3.73 times 10^(-2) $], [$5.60 times 10^2$],
      [Actuarial Example], [$5.75 times 10^3 plus.minus 4.71 times 10^0$], [$8.24 times 10^3 plus.minus 6.26 times 10^0$], [$2.65 times 10^3 plus.minus 2.17 times 10^0$], [$1.45 times 10^0 plus.minus 2.00 times 10^(-3)$], [$5.68 times 10^3$],
    ),
    caption: [Overview of the experiment results that measure performance of Excelerate and Excel in microseconds ($mu s$). Results are the average of $n=100000$. We also show the standard error.],
    placement: auto,
  )<table:results:performance>
] 

#figure({
  let complexity = (701, 85, 53, 5733, 3007)

  // let insertion     = (4540.0, 10300.0, 10700.0, 10100.0, 5750.0)
  // // let insertion_se  = (8.37,   19.3,    8.84,    8.36,    4.71)
  let excel_calc    = (4260.0, 2320.0, 2570.0, 12600.0, 8240)
  // let excel_calc_se = (2.68,   3.27,   4.80,   4.43,    0.626)
  // let extraction    = (3180.0, 3700.0, 4180.0, 3250.0, 2650.0)
  // // let extraction_se = (2.39,   3.16,   6.34,   1.92,   2.17)
  let excelerate_calc    = (5.12, 2.65, 0.407, 22.5, 1.45)
  // let excelerate_calc_se = (0.0152, 0.00375, 0.0428, 0.0373, 0.002)
  
  let excelerate-data = complexity.zip(excelerate_calc).chunks(1)
  // let insertion-data = complexity.zip(insertion)
  // let exctraction-data = complexity.zip(extraction)
  let calculation-data = complexity.zip(excel_calc).chunks(1)

  grid(columns: 2,
      gutter: 20pt,
    cetz.canvas({
      plot.plot(size: (6,3), x-tick-step: 1000, y-tick-step: 5, x-label: align(center, [no. Compute Units \ Excelerate]), y-label: [Time (in ms)],
      {
        plot.add(excelerate-data.at(0), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: red))
        plot.add(excelerate-data.at(1), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: orange))
        plot.add(excelerate-data.at(2), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: yellow))
        plot.add(excelerate-data.at(3), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: green))
        plot.add(excelerate-data.at(4), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: 2pt + red, fill: blue))

        plot.add(x => 0.003651871557961639* x + 1.6692250302690241, domain: (0, 6000), style: (stroke: (paint: gray, thickness: 1pt, dash: "dashed")))
        })
    }),
    cetz.canvas({
      plot.plot(size: (6,3), legend: (2, 6), x-tick-step: 1000, y-tick-step: 5000, x-label: align(center, [no. Compute Units \ _Excel_]), y-label: [Time (in ms)],
      {
        plot.add(calculation-data.at(0), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: red))
        plot.add(calculation-data.at(1), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: orange))
        plot.add(calculation-data.at(2), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: yellow))
        plot.add(calculation-data.at(3), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: green))
        plot.add(calculation-data.at(4), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: blue))
        plot.add(x => 1.7821908285638999 * x + 2583.678810637278, domain: (0, 6000), style: (stroke: (paint: gray, thickness: 1pt, dash: "dashed")))
        
        plot.add-legend("Monthly Budget", preview: () => {
          cetz.draw.rect((0,0), (1,1), fill: red)
        })
        plot.add-legend("Holiday Budget", preview: () => {
          cetz.draw.rect((0,0), (1,1), fill: orange)
        })
        plot.add-legend("Service Invoice", preview: () => {
          cetz.draw.rect((0,0), (1,1), fill: yellow)
        })
        plot.add-legend("Retirement Planner", preview: () => {
          cetz.draw.rect((0,0), (1,1), fill: green)
        })
        plot.add-legend("Actuarial Example", preview: () => {
          cetz.draw.rect((0,0), (1,1), fill: blue)
        })
      })
    }),
    
    // cetz.canvas({
    //   plot.plot(size: (6,3), 
    //   {
    //     plot.add(insertion-data, line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: none, fill: green))
    //   })
    // }),
    // cetz.canvas({
    //   plot.plot(size: (6,3), 
    //   {
    //     plot.add(calculation-data, line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: none, fill: green))
    //   })
    // }),
  )
  },
  caption: [A scatter plot of the completion time versus the number of compute units for each spreadsheet in Excelerate emitted code and Excel. Trendlines have been drawn to show the relationship. The _Actuarial Example_ is considered an outlier.],
  placement: bottom,
)<table:results:scatterplot>


Excelerate manages to achieve microsecond performance on many spreadsheets, while Excel takes around 2 milliseconds for the calculations. The Excel calculations take considerably longer than Excelerate. @table:results:performance shows the average speedup for every spreadsheet. The overhead of the COM interface is included in these figures. Taking the geometric mean, an average speedup of 1710x was observed. However, this includes the extreme speedups for both the simple _Service Invoice_ and _Actuarial Example_. The _Service Invoice_ is extremely simple, and investigating the _Actuarial Example_, we see that the chain that is present within the spreadsheet was not detected by Excelerate and as such, very verbose code was created.

@table:results:scatterplot shows the number of compute units against the time it takes to calculate for Excelerate and Excel. Apart from the _Actuarial Example_ (blue) spreadsheet, the relationship between compute time and the complexity seems linear. The plots in @table:results:scatterplot are similar, indicating that both methods handle the complexity in the same way. Based on the speedup values in @table:results:performance and the linear trends in @table:results:scatterplot, we infer a negative relationship between the speed-up and the complexity of the spreadsheet.

An interesting observation can be made for the _warmup_ for the Excelerate compiler. The first run of the Excelerate compiler is often 250 times slower than the next run due to the overhead of the JIT compiler. We do not include this in the performance measures since it is only the first run and it significantly skews the variability.

// Should I say something about the variance of the data?

#figure({
  let excel = (0, 1068740.3582, 256588.4453, 417533.4169)
  let excelerate = (85, 0, 41.1833, 0)
  
  let data1 = (
    ([Excel\ (1742862ms)], ..excel),
  )
  let data2 = (
    ([Excelerate\ (126ms)], ..excelerate),
  )
  align(left, [
  #cetz.canvas({
    chart.barchart(data1, 
      label-key: 0,
      value-key: (..range(1, 5)),
      size: (13, auto), 
      mode: "stacked", 
      labels: ([Compilation], [Insertion], [Calculation], [Extraction]), 
      x-label: [Time (in ms)], 
      bar-style: cetz.palette.new(colors: (orange, red, green, blue)),
      anchor: "bottom",
      legend: (10,5),
    )
  })
  #cetz.canvas({
    chart.barchart(data2, 
      label-key: 0,
      value-key: (..range(1, 5)),
      size: (13, auto), 
      mode: "clustered", 
      labels: ([Compilation], [Insertion], [Calculation], [Extraction]), 
      x-label: [Logarithmic Time (in ms)], 
      bar-style: cetz.palette.new(colors: (orange, red, green, blue)),
      anchor: "bottom",
      legend: none,
    )
  })
]
  )
  },
  caption: [A visualisation of the total amount of time it takes to execute 100000 invocations on the _Service Invoice_ spreadsheet. The _Compilation_ step only has to be run once. The graph is logarithmic to show that Excelerate is orders of magnitude faster.],
  placement: auto,
)<table:results:overhead>

@table:results:overhead shows the amount of time it takes to execute 100 000 invocations of the smallest spreadsheet: _Service invoice_. Excelerate is considerably faster than Excel, finishing compilation and calculation in the same time that calculation is completed. The compilation is about 85ms, accounting for 66.7% of the total wall time. Conversely, if we look at the composition of the performance of Excel, the insertion of data into the spreadsheet takes the longest, accounting for 61.3% of the total wall time. The actual calculation only takes 14.7% of the total wall time.

The insertion times for _Family Monthly Budget_ and _Expense Tracker_ are significantly higher than the calculation and extraction combined. @table:results:insertions describes the amount of insertions needed. The amount of time spent on insertions linearly correlate with the cost of insertions. This cost of insertions is chosen over simply taking the amount of insertions. Since ranges are more expensive to insert, as they need to transport more data, we count them as three cells.

#figure({
  let size = (9, 18, 4, 5, 6)
  
  let insertion     = (9070.0, 20700.0, 10700.0, 10100.0, 5750.0)
  
  let insertion-data = size.zip(insertion).chunks(1)

  cetz.canvas({
    plot.plot(size: (6,3), legend: (0,-2), x-label: [Insertion cost], y-label: [Time (in ms)], y-min: 0, y-tick-step: 5000,
    {
      plot.add(insertion-data.at(0), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: red))
      plot.add(insertion-data.at(1), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: orange))
      plot.add(insertion-data.at(2), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: yellow))
      plot.add(insertion-data.at(3), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: green))
      plot.add(insertion-data.at(4), line: "raw", style: (stroke: none), mark: "o", mark-style: (stroke: black, fill: blue))
      plot.add(x => 843.204334365325 * x + 4181.083591331266, domain: (0, 18), style: (stroke: (paint: gray, thickness: 1pt, dash: "dashed")))
      })
  })
  },
  caption: [A scatter plot of the amount of time it takes to insert the input into the Excel spreadsheet for $n=100000$. A linear regression line has been drawn. This indicates a linear relationship. The insertion cost is calculated as 3 units for ranges and 1 unit for cells.],
  placement: auto,
)<table:results:insertion>
