#import "../../utils/chapters.typ": chapter
#import "../../utils/spreadsheet.typ": spreadsheet
#import "@preview/fletcher:0.5.8" as fletcher: diagram, node, edge

#chapter("Compiling Excel")

#include "compiler-structure.typ"

#include "ahead-of-time-compilation.typ"

#include "structure-aware-compilation.typ"

= High Level Overview
Before we dive deeper into the intermediate representations, it is helpful to discuss the high level overview of the compiler. This section, we walk through the biggest steps of the compiler and relate the different steps and structural models to each other. In subsequent sections, we dive deeper in the actual structure of all the models. 

#include "examples/budget-example.typ"

#include "overview.typ"

#include "intermediate-representations/structural.typ"
#include "intermediate-representations/compute.typ"
#include "intermediate-representations/data.typ"
#include "intermediate-representations/code-layout.typ"


#pagebreak()

#align(
  center + horizon,
  [
    #line(length: 100%)

    This is the new version of Chapter 2. It is not finished and a lot of the content should be ported over to the new verison. The new version should have more examples, be more understandable and just be more enjoyable to read. Beware that there is totally new information and presentation in here! Please do not skip it :)
    
    #line(length: 100%)
  ] 
)

#pagebreak()

#show raw.where(block: true): it => {
  set text(font: "JetBrains Mono")

  // The raw block should be encased in a box
  align(left,
    block(it, fill: luma(97%), inset: 1em, radius: .25em, width: 100%)
  )
}

At first glance, an Excel workbook might seem like a collection of mere numbers and formulas, arranged in rows and columns. Yet, beneath this seemingly simple grid lies a carefully orchestrated structure that not only conveys data but also encodes lots of semantics and context.

Consider the family budget spreadsheet introduced in the previous chapter in @fig:family-budget:overview. Just looking at the data and formulas to calculate the full budget is unreadable. The formulas, structure and styles make the spreadsheet extra strong. As we will show in this chapter, translating the spreadsheet bears a lot of similarities. Translating this spreadsheet into C\# code is not just a matter of converting the underlying epxression. The bigger challenge lies in preserving the readability and implicit structure that gives the spreadsheet its meaning.

This chapter begins with the most straightforward approach, which we dub the trivial compiler. We show it is easy to compile a spreadsheet to code by treating the spreadsheet as a flat collection of formulas. The result is correct, but not absolutely not maintainable or idiomatic.

Recognizing this limitation, we then introduce a more robust approach: _Structural Compilation_. This approach captures the structure of the spreadsheet and guides the compilation process. Consequently, the compiler can produce code that is not only correct but also easier to read and maintain, preserving the spreadsheet’s original semantics.

In the remainder of this chapter, we explore the latter of this approach in detail, describing the compiler while walking down the steps. We cover the algorithms and intermediate representations necessary to support this structural compilation for Excel.

= The Trivial Route
The most trivial way to compile an Excel workbook is by extracting the underlying computational model and compiling it directly to C\# code. The underlying computational model is comprised of a graph of connected formulae, where formulae in different cells are connected through the use of references.
In this section, we briefly discuss the easiest way to compile an excel sheet and evaluate the output. As we will see in the rest of the thesis, we can use a different, more robust form of _structural compilation_ to compile the spreadsheet better.

#include "examples/budget-income.typ"

We walk through the example of a portion of the budget spreadsheet, shown in @sps:budget:income. We see it is a table that describes the three incomes of the family, with the expected (projected) and the actual amount of money that has been received this month. This is a basic example since there are not that many complexities in this spreadsheet. However, we will see that the simple compiler cannot even compile this simple example.

== Compute Units
For simplicities sake in this section, we will assume we already can easily access the contents of the cells. The first step or compiler pass of the trivial compiler is extracting the compute flow graph. However, unlike in other languages, in Excel has no explicit control flow, except for certain formulas. 

Hence, we construct a Compute Graph (which is similar but distinct from a compute flow graph), where we consider the contents, functions and operators to be nodes in the graph. We call the nodes in the graph _Compute Units_. The graph is undirected, and can have multiple roots. The outputs of the program are the roots of the graph. For instance, if we look at a different view of @sps:budget:income we see the formulae that computes the cell `D5`. If we take this as an output of our generated program, we can trace the dependencies that are needed to calculate D5, and their dependencies, etc... to generate a tree (or graph since we can re-use cells) of calculations. 

While we construct this tree, we convert every cell with a constant to a _constant_ compute unit. This compute unit cannot have dependencies and acts as a leaf of the graph. Cells `B2:C4` are all cells with constants, and will thus be converted to a constant compute unit. 

If a cell contains a formula, it is a different story. We need to be sure to fully represent the full formula with all of its _functions_ and their _arguments_, as well as operators and their arguments. As such, we also store a _function_ compute unit with dependencies as their _arguments_. Both the excel functions and arguments map to this compute unit. If a function uses a cell reference as argument, we look at that cell and use the generated subgraph of that cell as dependency.

#figure(
  {
    let excelGreen = rgb("#257835")
    let uvaRed = rgb("#BC0031")

    set text(fill: white, weight: "bold", size: 1em)
    // scale(
    diagram(
      edge-stroke: 1pt,
      node-corner-radius: 5pt,
      edge-corner-radius: 8pt,
      mark-scale: 60%,
  
      node((0,0), [*Sum*], name: <sum>, fill: excelGreen, shape: fletcher.shapes.hexagon),
  
      node((1.3,1), [-], name: <minus-1>, fill: orange, shape: fletcher.shapes.circle),
      node((0,1), [-],  name: <minus-2>, fill: orange, shape: fletcher.shapes.circle),
      node((-1.3,1), [-],  name: <minus-3>, fill: orange, shape: fletcher.shapes.circle),
  
      node((1.65, 2), [6000], name: <minus-1-arg1>, fill: uvaRed),
      node((1, 2), [5800], name: <minus-1-arg2>, fill: uvaRed),
      
      node((-0.3, 2), [1000], name: <minus-2-arg1>, fill: uvaRed),
      node((0.3, 2), [2300], name: <minus-2-arg2>, fill: uvaRed),
      
      node((-1.65, 2), [2500], name: <minus-3-arg1>, fill: uvaRed),
      node((-1, 2), [1500], name: <minus-3-arg2>, fill: uvaRed),
      
      edge(<minus-1>, <sum>, "-}>"),
      edge(<minus-2>, <sum>, "-}>"),
      edge(<minus-3>, <sum>, "-}>"),
      
      
      edge(<minus-3-arg1>, <minus-3>, "-}>"),
      edge(<minus-3-arg2>, <minus-3>, "-}>"),
      edge(<minus-1-arg1>, <minus-1>, "-}>"),
      edge(<minus-1-arg2>, <minus-1>, "-}>"),
      edge(<minus-2-arg1>, <minus-2>, "-}>"),
      edge(<minus-2-arg2>, <minus-2>, "-}>"),    
    )
    // x: 80%,
    // y: 50%
    // )
  },
  caption: [The compute graph generated from the formulae and dependencies of @sps:budget:income:formulae.],
  placement: auto
)<fig:family-budget:income-summary-compute-graph>

In the end we will construct a compute graph like in @fig:family-budget:income-summary-compute-graph. The `Sum` formula has three dependencies, which all have two constant dependencies. It is clear to see that this notation and intermediate representation describes the computation that is being done in @sps:budget:income.

== Compiling the code
When the compute graph is constructed, the code can be generated. The generated compute graph can be seen as one large deconstructed expression. The compute graph generated in @fig:family-budget:income-summary-compute-graph can be expressed as the expression: ``` SUM(2500-1000,1000-2300,5800-6000)```. Without any further compiler passes, we do not have any other options than to compile it to one single expression in C\#. 

Compiling the structure to C\# is straight-forward. We conduct a fold operation on the compute graph, converting the leaves to C\# constants. By induction, we then convert a node with their already converted dependencies to C\# code. For instance, the minus operation in @fig:family-budget:income-summary-compute-graph would be converted to the minus operator in C\#: `<A> - (<B>)`. Since the fold makes sure that the dependencies of the minus operation are already converted, we can just fill it in so we get `2500 - 1000`. Since C\# also support functional notation, even aggregation operations like `SUM` can be compiled. 

#figure(
```cs
public double Main() {
  return new int[] {2500-1000,1000-2300,5800-6000}.Sum();
}
```,
caption: [The compiled code using the _Trivial Compiler_.],
placement: top
)<code:trivial-compiler-result>

@code:trivial-compiler-result shows the compiled result. The whole computational model has been reduced to an expression. Evaluating this expression yields the same value as the value in cell `D5` in @sps:budget:income. We use the Language Integrated Query DSL provided by .NET to enable the functional language capabilities such as the self-explanatory `.Sum()` overload for the array type. 

== Missing the structure

Because we only consider the formulas of the computational model, the compiled code is just a reduction of the computational model to an expression. The goal of this thesis is to create readable, idiomatic code from Excel. Arguably, the example in @code:trivial-compiler-result is not idiomatic, and barely readable. For this example, it is doable, but bigger spreadsheets with many more calculations will produce one big expression that is unmaintainable.

We could introduce some refactoring techniques to make it a bit more readable, such as extracting variables @fowler_refactoring_2019. However, with nested formulae, it becomes difficult quite easily to determine when to extract a variable. If we do it too often, the code becomes too crowded with variables. Furthermore, when extracting variables at the wrong place, it might weaken the semantic value of that variable.
[Insert example of variable that means a different thing when you split it up or at least becomes less readable]

However, if we look at @sps:budget:income, we already can see a lot of the semantics is encoded in the spreadsheet structure. The meaning of the variables is described in the headers of the columns and rows of this table. Furthermore, we can infer a more general logic when looking at this structure. The `SUM` in `D5` does not just denote the sum of the range, but more likely means the total of the _difference_ column of the table. When taking this information into account, we can create a better conversion and compile more readable code. In this thesis, we introduce exactly this form of compilation, called _Structural Compilation_, that takes structural information and metadata into account in order to improve the readability of the emitted code.

= Structural Compilation

In traditional compilation, trivia like whitespace and comments are often discarded when the source code is converted to an abstract syntax tree (AST). When we convert the spreadsheet to the Compute Graph AST in the trivial compiler in the previous section, we saw that we lost the metadata of the structure of the spreadsheet. It also became apparent that the spreadsheet structure contains a lot of valuable data that can be used within the compilation.

Within source-to-source compilation, this _extra_ information like the comments in the source language is sometimes preserved. However, it is not used, mostly because that information is unstructured (it is text) and will therefore be difficult to obtain usable information in a reliable way. Within the excel grid, this information is always available.

Consequently, we introduce _structure-aware compilation_ in this thesis: compilation that uses cues of the structure and trivia of the source language to guide the compilation. This means that in essence, every source program should be able to compile to the target language, but the structure dictates the quality of the emitted code. 

#figure(
  spreadsheet(
    columns: 5,
    [6.000], [1.000], [2.500], [], [=A1-A2],
    [5.800], [2.300], [1.500], [], [=B1-B2],
    [], [], [], [], [=C1-C2],
    [TOTAL], [=SUM(E1:E3)]
  ),
  caption: [A semantically equivalent spreadsheet to @sps:budget:income, if we evaluate cell `B4`.],
  supplement: "Spreadsheet",
  placement: top,
)<sps:structural-compilation:non-idiomatic>

For instance, take the @sps:budget:income, which is an obvious table. Now compare this to @sps:structural-compilation:non-idiomatic, which is semantically equivalent but structurally totally different. The underlying computational model is the same. However, using structural compilation, the former spreadsheet will be compiled with more idiomatic code than the latter, because the compiler is guided by the structure of the table.

In order for this to work, it is vital to have an abstracted representation of the structure of the source program. This representation can be used by all subsequent passes to guide the process. Consequently, the compilation should begin with the extraction of the structure to a dedicated intermediate representation. This intermediate representation will be domain specific. 

In the context of the Excel compiler, we introduce the _Structural Model_, an intermediate representation that partially maps the Excel Workbook. This structural model is completely distinct from the underlying computational model, which is being modelled by a different intermediate representation.

== Structures

The grid of the Excel Spreadsheet can contain patterns of specialized structures that we can use for augmented compilation. Based on many idiomatic spreadsheets, mostly templates provided by Microsoft, we identified three separate structures. While these structures are based on the structures found in the idiomatic spreadsheets, they can also be generalized to other applications.

=== Tables

#figure(
  spreadsheet(
    columns: 5,
    [*Description*], [*Category*], [*Projected Cost*], [*Actual Cost*], [*Difference*],
    [Extracurricular activities], [Children], [40.00], [40.00], [=D2-C2],
    [Sporting Events], [Entertainment], [0], [40.00], [=D3-C3],
    [Fuel], [Transportation], [450], [0], [=D4-C4],
    [Parking Fees], [Transportation], [], [], [=D5-C5]
  ),
  caption: [An example of a table structure found in an excel sheet. Notice how in a table, it is possible to omit values, just like in Excel.],
  supplement: "Spreadsheet",
)<sps:structures:table>

Tables are essential structures in Excel. Almost all spreadsheets contain some sort of table. That is not surprising, since spreadsheets are tabular by nature, so chances are high that tables are created in this format. 

The table structure we recognize is a structure with clear division between the headers and data rows. The table represents a class of entities. Each row represents a distinct entity, and each column corresponds to a specific attribute or metric of that class. Columns can contain data, or be computed, just like an Excel table (see @subsec:excel-overview:tables).

An important property of the table structure is that the rows are independent of each other. This mostly applies to the calculated columns such as the _Difference_ column in the E column in @sps:structures:table. The dependencies of every cell in this column is only 'horizontal', meaning that they depend on cells in their respective rows.

A common pattern that we see is a operation on a whole column of the table. For example, in the _Monthly Expenses_ spreadsheet seen in @fig:family-budget:monthly-expenses, one of the operations is to sum up all the projected costs and actual costs. Without the structure, the code would look like a bunch of variables being put into an array, and an operation being performed on them. 
[Create example here to illustrate it with code.]
With the table structure, the data is abstracted away, and we can cleanly use columns in the data. 

=== Chain-Tables

Chains are just like tables, they contain data in a tabular format: with a clear division of headers and data. However, an important difference between a chain table and a normal table, is the dependence of the columns: where the rows of a table have to be independent, we require the rows of the chain to be dependent.

#figure(
  spreadsheet(
    columns: 4,
    [], [*Interest*], [*Money*], [*Deposit*],
    [May], [], [10 000], [],
    [June], [=$0.01 dot #[C2]$], [=C2+B3+D3], [500],
    [July], [=$0.01 dot #[C3]$], [=C3+B4+D4], [500],
    [August], [=$0.01 dot #[C4]$], [=C4+B5+D5], [500],
  ),
  caption: [An example of a chain-table. The chain is visible in the _Interest_ (B) and _Money_ (C) column of the table, where the interest calculated is based on the previous month.],
  supplement: "Spreadsheet",
)<sps:structures:chain>

The spreadsheet in @sps:structures:chain shows three columns that are part of the chain. For instance, we see that to calculate the total value in cell C4, we not only use the D4 and B4 values in row 4, but also use the total bank account value of the previous month in C3. 

[Introduce spreadsheet with drawn dependencies (this is a bit more difficult in Typst)]

This pattern of calculations that are dependent on previous calculations are quite common, and are often also called _recursive calculations_, since they depend on a previous value. The way Excel calculates them is using tail recursion. To illustrate this, we will calculate C4 in @sps:structures:chain: In order to calculate this cell, we need C3 and B4 and D4. B4 also needs C3. To calculate C3, we first need to calculate C2. C2 is just a constant value, so the chain stops here. Especially in programming, this is an anti-pattern since we can replace the calculation with a simple loop that calculates C4. [When we use an illustration here, we could maybe showcase that instead of top-down, the loop goes down-to-top]

=== Repositories

#figure(
  spreadsheet(
    columns: 5,
    [0.9324],[0.2353],[0.7263], [0.1825], [0.2182],
    [0.5876],	[0.7531],	[0.2743], [0.5404], [0.0610],
    [0.6234],	[0.7195],	[0.1423], [0.2772], [0.4714],
    [0.3861],	[0.9621],	[0.8352], [0.6922], [0.9480],
    [0.5937], [0.5274], [0.0450], [0.5837], [0.7037],
  ),
  caption: [A real world snippet of a data repository based on a spreadsheet published by DNB. It is part of a spreadsheet filled with data like this.],
  supplement: "Spreadsheet",
)<sps:structures:repositories>

Where chains and tables are quite structured, repositories look like blobs of unstructured data. They are often created in separate worksheets within a workbook and denote a collection of related data. 

== One or more IRs

- Explain the need for more IRs
- Using one IR for everything, also the structure is near impossible.
- Multiple IRs allow for more granular passes, which make the compiler more maintainable and the code more readable.
- Provide a grand overview of the Excelerate compiler.
#image("../../images/excel-compiler-overview.png")

- Briefly describe and introduce the reader to IRs and Areas of the compiler.
- Announce the structure of the rest of the chapter to the reader

= Modeling the workbook

- Introduce the first area of the compiler: Structure

== Reading the Excel File

- Provide an in-depth example of what the excel file is, and how we read it to our own intermediate representation.
- Allow a quick mention of the EPPlus library, which we are using for this research.
- Mention the XLParser library and the research paper behind it.
- Give an non-formal introduction of the structure IR.
- Use the budget spreadsheet as an example.

== Extracting Areas & Implicit Structures

- Describe what Areas are: continuous regions in a spreadsheet of possible structure data.
- Describe the algorithm for finding structures.
- Describe the algorithm for detecting the table, chain and repository structures.
  - Use the Expenses and Savings of the budget spreadsheet as an example.

== Formal definition of the Structure IR

- Describe the IR formally, but not in the same sense as the 

= Deriving the Logic

- Introduce the reader to the second area of the compiler: Compute
- Remind the reader of the Compute IR introduced and convey we will change it up a bit.

== Compute Grid

- Introduce the reader to the compute grid and the references in the compute IR.
- Describe the pass from the structure to the compute grid.
  - Mention the structures and that they get special treatment in this IR.
- Describe the different references possible with an example of the budget spreadsheet.
- An interesting observation: every converted formula tree is also a tree in the compute grid.
- Describe other passes that happen at this stage (if any, right now there aren't any).

== Compute Graph

- Introduce the compute graph
- Describe the link pass, where we went from the grid to the graph.
- Describe the Compute IR formally

== Type Resolution

- Describe the type resolution algorithm, which is a basic fold on the tree of compute graph.
- Showcase type resolution of an expression in the budget spreadsheet example.
- Describe all the different type resolution with pretty figures.

= Extracting the Data

- Introduce the reader to the third area of the compiler: Data

_note to self:_ this is the area I am still not sure about how to represent it the best. The implementation works right now, but I am not sure if it is robust.

== Variable Separation

- Describe the need for the data IR
  - Variability in the data
  - Inputs and outputs
  - Simplify compute model
- Introduce inputs (and outputs formally).
- Explain it with an example like the expenses or incomes table in the budget spreadsheet.

== The Data IR

- Formally introduce the Data IR

= Generating Readable Code

- Introduce the reader to the final area of the compiler: Code Layout
- Clearly state the need for this area and IR

== Layout

- Introduce the Layout IR formally
- Explain it with the budget spreadsheet.

== Emission

- Introduce the Roslyn API formally
- Explain the pass from layout to Roslyn API and the difficulties there.
 - When to insert trivia to make it more readable

= Discussion of the Compiler Design

- This is room for any discussions of the compiler that could not fit into the story.
- The above should be read like a single story
- But of course, some things might not fit into it well, so we may want to have a separate section for it.