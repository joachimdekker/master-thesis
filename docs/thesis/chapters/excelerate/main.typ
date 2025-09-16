#import "../../utils/chapters.typ": chapter
#import "../../utils/spreadsheet.typ": spreadsheet
#import "../../utils/cite-tools.typ": citeauthor

// Setup 

#show raw.where(block: false): it => {
    set text(font: "JetBrains Mono", size: 0.9em)
    
    box(it, 
    fill: luma(95%), 
    inset: (x: 4pt, y: 0pt),
    outset: (y: 4pt),
    radius: 2pt,)
}

#show figure: it => {
  show raw.where(block: true): r => {
    set text(font: "JetBrains Mono", size: 0.7em)
  
    // The raw block should be encased in a box
    align(left,
      block(r, fill: luma(97%), inset: 1em, radius: .25em, width: 100%)
    )
  }

  show raw.where(block: false): r => {
    set text(font: "JetBrains Mono", size: 1em)
    r
  }
  
  it
}


#chapter("Excelerate")

The 'basic' compiler introduced in the previous chapter had difficulties emitting idiomatic code. However, as we discussed in @sec:basic-compiler:reflection, the desired idiomatic code had a lot of structural similarities with the spreadsheet it should represent. We found that these structures could help compilation and improve the readability of the code. In this chapter, we introduce Excelerate and its _structure-aware compilation_. This technique uses structures embedded in the spreadsheet as heuristics during compilation.

At first glance, an Excel workbook might seem like a collection of mere numbers and formulas, arranged in rows and columns. Yet, beneath this seemingly simple grid lies a carefully orchestrated structure that not only conveys data but also encodes semantics and context.

Consider the family budget spreadsheet introduced in the introduction in @fig:family-budget:overview. The data and formulas alone do not communicate the semantics of the spreadsheet well. The styles and structures used in the spreadsheet is what transforms these pieces of data into an understandable presentation. As such, the challenge lies in preserving the readability and implicit structure that gives the spreadsheet its meaning.

In this chapter, we start by introducing _structure-aware compilation_ and covering the structures we identified. Then, we proceed to list the changes that need to be made to the compiler by giving a high level overview of what needs to happen to compile using _structure-aware compilation_. The rest of the chapter is dedicated to the specifics of changes needed in each phase.

= Structure-aware compilation

When the 'basic' compiler converted the _Structure Model_ to the _Compute Graph_, a lot of metadata about the spreadsheet was lost. This metadata---in the form of the 2D formation on the _Spreadsheet_ grid---contains valuable data that could be used as heuristics in compilation. 

In this thesis, we introduce _structure-aware compilation_: a compilation technique that uses knowledge of the structure of the source program to guide code generation to the target language. In essence, every source program should be able to compile to the target language, but _structure-aware compilation_ tries to preserve the structure of the source program in the emitted code.

#figure(
  text(size: 0.7em, 
  grid(columns: 2,
       gutter: 3em,
        spreadsheet(
          columns: 4,
          [], [*Projected*], [*Actual*], [*Difference*],
          [Income 1], [6.000], [5.800], [`=C2-B2`],
          [Income 2], [1.000], [2.300], [`=C3-B3`],
          [Extra Income], [2.500], [1.500], [`=C4-B4`],
          [_TOTAL_], [], [], [`=SUM(D2:D4)`]
        ), 
        spreadsheet(
          columns: 5,
          [6.000], [1.000], [2.500], [], [=A1-A2],
          [5.800], [2.300], [1.500], [], [=B1-B2],
          [], [], [], [], [=C1-C2],
          [TOTAL], [=SUM(E1:E3)]
        ))),
  caption: [Two computationally equivalent, but structurally different spreadsheets, if the left spreadsheet takes `D5` as output and the right takes `B4` as output. Both cells compute the exact same value, through the same computations.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:structure-aware-compilation:table>

For instance, take @sps:structure-aware-compilation:table. The left spreadsheet obviously contains a table. The right spreadsheet is computationally equivalent but structurally totally different. The underlying computational model is the same. However, using structure-aware compilation, the former spreadsheet should be compiled with more idiomatic code than the latter, because the compiler will be guided by the structure of the table.

That said, in order for _structure-aware compilation_ to work, we need to identify specific structures that could help structure the emitted code. Within the grid of the _Spreadsheet_, we should find patterns of structures. 

== Types of Structures

Using the Microsoft Create Excel Template repository, two spreadsheets were identified. The Microsoft Create Excel Template repository contains spreadsheets that are well-formatted, follow the "best-practices", model spreadsheets for different industries, and often show patterns that users adopt in their own spreadsheets. The patterns that occurred the most were the _Table_ like we saw in @sps:structure-aware-compilation:table and the _Chain_ that models a recurring operation like in @sps:structure-reflection:spreadsheet2. 

Another pattern was found in spreadsheets provided by DNB, which are more focussed on an actuarial context. Here, a lot of data of a single variable in a computation would reside in one spreadsheet. This pattern, which we call the _Data Pond_, was not implemented because of time.

#figure(
  spreadsheet(
    columns: 5,
    [0.9324],[0.2353],[0.7263], [0.1825], [0.2182],
    [0.5876],	[0.7531],	[0.2743], [0.5404], [0.0610],
    [0.6234],	[0.7195],	[0.1423], [0.2772], [0.4714],
    [0.3861],	[0.9621],	[0.8352], [0.6922], [0.9480],
    [0.5937], [0.5274], [0.0450], [0.5837], [0.7037],
  ),
  caption: [A real world snippet of a _Data Pond_ based on a spreadsheet published by DNB. This sheet, called '7_Renteparameter_phi_N' represents a single variable in a larger equation. It is part of a workbook filled with data like this.],
  supplement: "Spreadsheet",
)<sps:structures:data-pond>

//We define an idiomatic spreadsheet as a workbook that embodies the “best-practice” conventions of everyday Excel use—formulas, tables, and layout patterns that reflect how typical users structure real-world workbooks. Publicly available Microsoft Excel templates are strong candidates for analysis because they are curated by Microsoft to model practical tasks (budgets, schedules, inventories) and therefore showcase the recurring design patterns that ordinary users tend to adopt.

=== Tables

#figure(
  spreadsheet(
    columns: 5,
    hasFooter: true,
    text([_*Expenses*_], size: 1.2em), [], [], [], [],
    [*Description*], [*Category*], [*Projected Cost*], [*Actual Cost*], [*Difference*],
    [Extracurricular activities], [Children], [40.00], [40.00], [=D2-C2],
    [Sporting Events], [Entertainment], [0], [40.00], [=D3-C3],
    [Fuel], [Transportation], [450], [0], [=D4-C4],
    [Parking Fees], [Transportation], [], [], [=D5-C5],
    [_TOTAL_], [], [=SUM(C2:C5)], [=SUM(D2:D5)], [=SUM(E2:E5)],
  ),
  caption: [An example of a table structure found in a _Spreadsheet_. Cell `A1` denotes the title, range `A2:E2` describes the headers of the table. The actual data can be found in between the headers and the column in `A3:E6`. Finally, the footer is described in `A7:E7`.],
  supplement: "Spreadsheet",
)<sps:structures:table>

Tables are essential in Excel. Almost all _Spreadsheets_ contain some sort of Table. This is not surprising given the inherent tabular presentation of the Excel grid.

The table pattern that was often observed was a rectangular region in the _Spreadsheet_ containing data, a header and sometimes a title or a footer. The columns of the table may contain data, or they contain a formula. If a column has the same formula in every row, that column is a _Computed Column_. 

An important distinction between a _Table_ and a _Chain_ is the dependencies between columns. While the table 

Semantically, a table represents a list of objects. These objects are described by the data in the rows. For instance, in the table in @sps:structures:table, the table describes a list of expenses. The properties of the objects are determined by the column names. In the aforementioned table, this means that the properties of an expense are a description, a category, the projected cost, the actual cost and the difference.

= Changes to the Compiler<sec:excelerate:compiler-changes>

#figure(
  image("image.png"),
  caption: [The new phases of the compiler.]
)

= Finding the Structures

= Embedding the Structures

= Coding the Structures