#import "../../utils/chapters.typ": chapter
#import "../../utils/spreadsheet.typ": spreadsheet
#import "@preview/fletcher:0.5.8" as fletcher: diagram, node, edge
#import "../../utils/cite-tools.typ": citeauthor

#chapter("Compiling Excel")

#include "compiler-structure.typ"

#include "ahead-of-time-compilation.typ"

#include "structure-aware-compilation.typ"

= High Level Overview
Before we dive deeper into the representations, it is helpful to discuss the high level overview of the compiler. This section, we walk through the biggest steps of the compiler and relate the different steps and structural models to each other. In subsequent sections, we dive deeper in the actual structure of all the models.

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

#show figure: it => {
  show raw.where(block: true): it => {
    set text(font: "JetBrains Mono", size: 0.7em)
  
    // The raw block should be encased in a box
    align(left,
      block(it, fill: luma(97%), inset: 1em, radius: .25em, width: 100%)
    )
  }
  it
}


At first glance, an Excel workbook might seem like a collection of mere numbers and formulas, arranged in rows and columns. Yet, beneath this seemingly simple grid lies a carefully orchestrated structure that not only conveys data but also encodes lots of semantics and context.

Consider the family budget spreadsheet introduced in the previous chapter in @fig:family-budget:overview. Just looking at the data and formulas to calculate the full budget is unreadable. The formulas, structure and styles make the spreadsheet extra strong. As we will show in this chapter, translating the spreadsheet bears a lot of similarities. Translating this spreadsheet into C\# code is not just a matter of converting the underlying epxression. The bigger challenge lies in preserving the readability and implicit structure that gives the spreadsheet its meaning.

This chapter begins with the most straightforward approach, which we dub the trivial compiler. We show it is easy to compile a spreadsheet to code by treating the spreadsheet as a flat collection of formulas. The result is correct, but not absolutely not analyzable or _idiomatic_. In the upcoming subsection, we explain why this is the case, and what we view as idiomatic code.

Recognizing this limitation, we then introduce a more robust approach: _structure-aware compilation_. This approach captures the structure of the spreadsheet and guides the compilation process. Consequently, the compiler can produce code that is not only correct but also easier to read and maintain, preserving the spreadsheet’s original semantics.

In the remainder of this chapter, we explore the latter of this approach in detail, describing the compiler while walking down the steps. We cover the algorithms and intermediate representations necessary to support this structure-aware compilation for Excel.

= The Trivial Route<sec:trivial-compiler>
The most trivial way to compile an Excel workbook is by extracting the underlying computational model and compiling it directly to C\# code. The underlying computational model is comprised of a graph of connected formulae, where formulae in different cells are connected through the use of references.
In this section, we briefly discuss the easiest way to compile an excel sheet and evaluate the output. As we will see in the rest of the thesis, we can use a different, more robust form of _structure-aware compilation_ to compile the spreadsheet better.

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
  },
  caption: [The compute graph generated from the formulae and dependencies of @sps:budget:income:formulae.],
  placement: auto
)<fig:family-budget:income-summary-compute-graph>

In the end we will construct a compute graph like in @fig:family-budget:income-summary-compute-graph. The `Sum` formula has three dependencies, which all have two constant dependencies. It is clear to see that this notation and intermediate representation describes the computation that is being done in @sps:budget:income.

== Compiling the code
When the compute graph is constructed, the code can be generated. The generated compute graph can be seen as one large deconstructed expression. The compute graph generated in @fig:family-budget:income-summary-compute-graph can be expressed as the expression: ``` SUM(2500-1000,1000-2300,5800-6000)```. While we can extract some of the operations in this expression to make the code more legible, it will be come more difficult to read when the expression is larger. Furthermore, the  

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

However, if we look at @sps:budget:income, we already can see a lot of the semantics is encoded in the spreadsheet structure. The meaning of the variables is described in the headers of the columns and rows of this table. Furthermore, we can infer a more general logic when looking at this structure. The `SUM` in `D5` does not just denote the sum of the range, but more likely means the total of the _difference_ column of the table. When taking this information into account, we can create a better conversion and compile more readable code. In this thesis, we introduce exactly this form of compilation, called _structure-aware compilation_, that takes structural information and metadata into account in order to improve the readability of the emitted code.

= Structure-aware compilation

In traditional compilation, trivia like whitespace and comments are often discarded when the source code is converted to an abstract syntax tree (AST). When we convert the spreadsheet to the Compute Graph AST in the trivial compiler introduced in the previous section, we saw that we lost the metadata of the structure of the spreadsheet. It also became apparent that the spreadsheet structure contains a lot of valuable data that can be used within the compilation.

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

For instance, take @sps:budget:income, which is an obvious table. Now compare this to @sps:structural-compilation:non-idiomatic, which is semantically equivalent but structurally totally different. The underlying computational model is the same. However, using structure-aware compilation, the former spreadsheet will be compiled with more idiomatic code than the latter, because the compiler is guided by the structure of the table.

In order for this to work, it is vital to have an abstracted representation of the structure of the source program. This representation can be used by all subsequent passes to guide the process. Consequently, the compilation should begin with the extraction of the structure to a dedicated intermediate representation. This intermediate representation will be domain specific. 

In the context of the Excel compiler, we introduce the _Structural Model_, an intermediate representation that partially maps the Excel Workbook. This structural model is completely distinct from the underlying computational model, which is being modelled by a different intermediate representation.

== Structures

The grid of the Excel Spreadsheet can contain patterns of specialized structures that we can use for augmented compilation. Based on many idiomatic spreadsheets, mostly templates provided by Microsoft, we identified three separate structures. While these structures are based on the structures found in the idiomatic spreadsheets, they can also be generalized to other applications.

#figure(
  spreadsheet(
    columns: 5,
    hasFooter: true,
    [*Description*], [*Category*], [*Projected Cost*], [*Actual Cost*], [*Difference*],
    [Extracurricular activities], [Children], [40.00], [40.00], [=D2-C2],
    [Sporting Events], [Entertainment], [0], [40.00], [=D3-C3],
    [Fuel], [Transportation], [450], [0], [=D4-C4],
    [Parking Fees], [Transportation], [], [], [=D5-C5],
    [TOTAL], [], [=SUM(C2:C5)], [=SUM(D2:D5)], [=SUM(E2:E5)],
  ),
  caption: [An example of a table structure found in an excel sheet. Notice how in a table it is possible to omit values, just like in Excel.],
  supplement: "Spreadsheet",
)<sps:structures:table>

=== Tables<subsec:structures:tables>
Tables are essential structures in Excel. Almost all spreadsheets contain some sort of table. That is not surprising, since spreadsheets are tabular by nature, so chances are high that tables are created in this format. 

The table structure we recognize is a structure with clear division between the headers and data rows. The table represents a class of entities. Each row represents a distinct entity, and each column corresponds to a specific attribute or metric of that class. Columns can contain data, or be computed, just like an Excel table (see @subsec:excel-overview:tables).

An important property of the table structure is that the rows are independent of each other. This mostly applies to the calculated columns such as the _Difference_ column in the E column in @sps:structures:table. The dependencies of every cell in this column is only 'horizontal', meaning that they depend on cells in their respective rows.

A common pattern that we see is a operation on a whole column of the table. For example, in the _Monthly Expenses_ spreadsheet seen in @fig:family-budget:monthly-expenses, one of the operations is to sum up all the projected costs and actual costs. Without the structure, the code would look like a bunch of variables being put into an array, and an operation being performed on them. 
[Create example here to illustrate it with code.]
With the table structure, the data is abstracted away, and we can cleanly use columns in the data. 


#figure(
  spreadsheet(
    columns: 3,
    hasHeader: true,
    hasFooter: true,
    hasTitle: true,
    table.cell(text("Header", size: 2em, weight: "bold")), [], [],
    [*Column 1*], [*Column 2*], [*Calculated Column*],
    [...], [...], [\=Op(A3, B3)],
    [...], [...], [\=Op(A3, B3)],
    [...], [...], [\=Op(A3, B3)],
    [=AVG(...)], [=SUM(...)], [=MAX(...)]
  ),
  caption: [An example of a table structure found in an excel sheet. Notice how in a table it is possible to omit values, just like in Excel.],
  supplement: "Spreadsheet",
  placement: auto,
)

==== Structure
A table has a predefined structure. The table is composed of at least one element: the data. Apart from the data, the table can be expanded with a title, a header describing names for the columns in the table, and a table footer. The table footer may only contain aggregation operations on the range of the column in the data column, with the exception of string column. A string column must contain a string value or no value in the bottom.

We try to clearly convey the different parts of the structure using typography in our representation of the tables. Titles are larger and column headers are in bold. The footer of the table has been made clear by a thicker gap between the data and the footer.

=== Chain-Tables<subsec:structures:chain>
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
  placement: auto,
  supplement: "Spreadsheet",
)<sps:structures:chain>

The spreadsheet in @sps:structures:chain shows three columns that are part of the chain. For instance, we see that to calculate the total value in cell C4, we not only use the D4 and B4 values in row 4, but also use the total bank account value of the previous month in C3. 

[Introduce spreadsheet with drawn dependencies (this is a bit more difficult in Typst)]

This pattern of calculations that are dependent on previous calculations are quite common, and are often also called _recursive calculations_, since they depend on a previous value. The way Excel calculates them is using tail recursion. To illustrate this, we will calculate C4 in @sps:structures:chain: In order to calculate this cell, we need C3 and B4 and D4. B4 also needs C3. To calculate C3, we first need to calculate C2. C2 is just a constant value, so the chain stops here. Especially in programming, this is an anti-pattern since we can replace the calculation with a simple loop that calculates C4. [When we use an illustration here, we could maybe showcase that instead of top-down, the loop goes down-to-top]


#figure(
  spreadsheet(
    columns: 3,
    hasHeader: true,
    hasFooter: true,
    hasTitle: true,
    table.cell(text("Header", size: 2em, weight: "bold")), [], [],
    [*Column 1*], [*Column 2*], [*Calculated Column*],
    [], [_start value_], [],
    [], [_start value 2_], [],
    [...], [...], [\= ...],
    [...], [...], [\= ...],
    [...], [...], [\= ...],
    [=AVG(...)], [=SUM(...)], [=MAX(...)]
  ),
  caption: [An example of a table structure found in an excel sheet. Notice how in a table it is possible to omit values, just like in Excel.],
  supplement: "Spreadsheet",
  placement: auto,
)

==== Structure
The structure of the chain-table is similar to the normal table. The chain-table can have column headers, a title and a footer with aggregation. Besides that, the recursive nature of the calculations of the require an extra section next to the data section. This _initial data rows_ contain the initial data to start the recursive definition and can be seen as the base-cases of the recursive definition.

// === Repositories

// #figure(
//   spreadsheet(
//     columns: 5,
//     [0.9324],[0.2353],[0.7263], [0.1825], [0.2182],
//     [0.5876],	[0.7531],	[0.2743], [0.5404], [0.0610],
//     [0.6234],	[0.7195],	[0.1423], [0.2772], [0.4714],
//     [0.3861],	[0.9621],	[0.8352], [0.6922], [0.9480],
//     [0.5937], [0.5274], [0.0450], [0.5837], [0.7037],
//   ),
//   caption: [A real world snippet of a data repository based on a spreadsheet published by DNB. It is part of a spreadsheet filled with data like this.],
//   supplement: "Spreadsheet",
// )<sps:structures:repositories>

// Where chains and tables are quite structured, repositories look like blobs of unstructured data. They are often created in separate worksheets within a workbook and denote a collection of related data. Repositories are matrixes of pure data that are used elsewhere in the workbook. 

// Often, we see that repositories have implicit meaning by row and column. For instance in @sps:structures:repositories, the rows represent scenarios and the columns represent time steps. Often that meaning is described in external documents or other ways the Excel compiler cannot understand. This means that we cannot extract further meaning besides it's matrix structure or the name of the worksheet.

// == One or more IRs

// The case of the trivial compiler showed that compiling Excel models to readable code is not easy. When using only the Compute Unit IR, we are able to accurately describe the model, but it does not produce readable, idiomatic code. As we saw in this chapter, we can use Structural Computation to provide more context of the Excel structure to the compiler and we argued for the introduction of more IRs. Before we continue and explore the IRs in depth, we first wanted to briefly discuss the importance of using more IRs instead of one.

// === What exactly is an IR?
// While we briefly discussed and gave a conceptual overview of the IRs, the reader may already have a conceptual understanding of what an IR is. 

// An _Intermediate Representation_ (IR) is a language in which we model information about a system. The language should be expressive enough to accommodate for all features of the languages. For instance, the C\# language uses an intermediate representation called the _Intermediate Language_. All .NET languages such as Visual Basic and F\# compile to this intermediate representation. This representation is then compiled to the specified platform. As such, this saves the compiler maintainers from doing duplicate work when compiling a language to specific platforms.

// An often-used form of representation is the _abstract syntax tree_ @johnson_intermediate_2010. This high level abstraction represents a parse tree, with a few modifications. For instance, intermediate nodes may be combined into new nodes and a lot of the terminals found in the parse tree are gone. 
// // Insert an example here!

// Within an compiler, it is always a question whether to use one big IR or 
// multiple IRs that compose together to compile the code. In the next section we discuss this, and give an argument as to why using multiple IRs is better than using one IR.

// === One IR
// First of all, we must acknowledge that it is possible to use one IR. It is possible to encode everything onto one structure.

// When using a single IR, 

// - Explain the need for more IRs
// - Using one IR for everything, also the structure is near impossible.
// - Multiple IRs allow for more granular passes, which make the compiler more maintainable and the code more readable.

= Excelerate
#figure(image("../../images/excel-compiler-overview.png"),
caption: [Overview of the Excelerate compiler. The phases have been given different colors.])

In this thesis, we introduce Excelerate: the excel compiler that compiles Excel code to C\#. Like we have already discussed in the previous chapter, Excelerate uses structure-aware compilation in order to emit more readable code. Excelerate uses multiple intermediate representations to achieve this. In this subsection, we provide a high-level overview of the compiler and introduce the phases and IRs.

== High-level overview

#figure(
  spreadsheet(
    columns: 4,
    [], [Projected], [Actual], [Difference],
    [Income 1], [6.000], [5.800], [=C2-B2],
    [Income 2], [1.000], [2.300], [=C3-B3],
    [Extra Income], [2.500], [1.500], [=C4-B4],
    [TOTAL], [], [], [=SUM(D2:D4)]
  ),
  caption: [The formulae behind @sps:budget:income. It is clear that there is a relationship between the Projected and Actual column of this table. ],
  supplement: "Spreadsheet",
)<sps:overview:start>

Let us start with the same example we saw in @sec:trivial-compiler of the spreadsheet about the income of a family. Instead of directly looking at the formulae and piecing them together, we instead use structure-aware compilation to look for structures. In the first phase of the compiler, the _Structural Phase_, we extract the _Structural Model_ out of the Excel Sheet.

The structural model is an extraction of the content and formatting of the cells of the excel spreadsheets into a two-dimensional grid, essentially copying the spreadsheet. However, this is not the only pass within the structural phase. Another important pass is structure detection. It is obvious that @sps:overview:start is structured like a table. In the structure detection pass, the compiler detects areas with possible structures and tries to convert them to the appropriate structure. In a subsequent step, we detect the type of columns

At the end of the structure phase, the compiler has an intermediate representation containing the full spreadsheet plus the found structures. So for the example in @sps:overview:start, we have the following intermediate representation. 

#figure(
  grid(columns: 2, spreadsheet(
    columns: 4,
    [], [Projected (data)], [Actual (data)], [Difference _`f(x)`_],
    [Income 1], [6.000], [5.800], [`=C2-B2`],
    [Income 2], [1.000], [2.300], [`=C3-B3`],
    [Extra Income], [2.500], [1.500], [`=C4-B4`],
    [TOTAL], [], [], [`=SUM(D2:D4)`]
  ),
  gutter: 1em,
  align(left, [
    #text([*Structures*], size: 1.2em)
    + _Table_ at A1:D5
  ])),
  caption: [The representation after the _Structure Phase_: the full _Structural Representation_. ],
  supplement: "Figure",
)<fig:overview:structure-representation>

Afterwards, we extract the computational flow and the data in the spreadsheet in separate phases. The computational flow is being represented by the _Compute Model_, which we already partially covered in @sec:trivial-compiler. This model covers the computational flow along with special constructs based on the structures found in the previous phase. This phase looks at the specified output cells, and then recursively extracts the dependencies.

Due to the structure-aware compilation, the structures found in the previous phase are converted to constructs that are embedded into the compute model. As such, references to a column in a table such as in `D5` are referencing the whole column of that construct. In the end this results in the following intermediate representation:

#figure(
  {
    let excelGreen = rgb("#257835")
    let uvaRed = rgb("#BC0031")

    set text(fill: black, weight: "bold", size: 1em)
    // scale(
    diagram(
      edge-stroke: 1pt,
      node-corner-radius: 5pt,
      edge-corner-radius: 8pt,
      mark-scale: 60%,
  
      node((0,0), [*Sum*], name: <sum>, fill: excelGreen, shape: fletcher.shapes.hexagon),

      let colRefText = [
        TableA1D5/Difference
      ],
      node((0,1), colRefText, name: <column-ref>, fill: purple.transparentize(50%), shape: fletcher.shapes.rect,),
  
      // node((1.3,1), [-], name: <minus-1>, fill: orange, shape: fletcher.shapes.circle),
      // node((0,1), [-],  name: <minus-2>, fill: orange, shape: fletcher.shapes.circle),
      // node((-1.3,1), [-],  name: <minus-3>, fill: orange, shape: fletcher.shapes.circle),
  
      // node((1.65, 2), [6000], name: <minus-1-arg1>, fill: uvaRed),
      // node((1, 2), [5800], name: <minus-1-arg2>, fill: uvaRed),
      
      // node((-0.3, 2), [1000], name: <minus-2-arg1>, fill: uvaRed),
      // node((0.3, 2), [2300], name: <minus-2-arg2>, fill: uvaRed),
      
      // node((-1.65, 2), [2500], name: <minus-3-arg1>, fill: uvaRed),
      // node((-1, 2), [1500], name: <minus-3-arg2>, fill: uvaRed),
      
      // edge(<minus-1>, <sum>, "-}>"),
      // edge(<minus-2>, <sum>, "-}>"),
      // edge(<minus-3>, <sum>, "-}>"),
      edge(<column-ref>, <sum>, "-}>"),

      node(enclose: ((1,0), (2.5, 1.25)), fill: purple.transparentize(90%)),
      
      node((1, 0), text([TableA1D5], size: 1.2em)),
      node((1,1), [Difference]),

      // Difference
      node((1.5,1), [-], fill: orange, shape: fletcher.shapes.circle, name: <diff-minus>),
      node((2.5,0.75), [Actual], fill: uvaRed.transparentize(50%), name: <diff-actual>),
      node((2.5,1.25), [Projected], fill: uvaRed.transparentize(50%), name: <diff-projected>),

      edge(<diff-minus>, <diff-actual>, "<{-"),
      edge(<diff-minus>, <diff-projected>, "<{-")
      
      
      // edge(<minus-3-arg1>, <minus-3>, "-}>"),
      // edge(<minus-3-arg2>, <minus-3>, "-}>"),
      // edge(<minus-1-arg1>, <minus-1>, "-}>"),
      // edge(<minus-1-arg2>, <minus-1>, "-}>"),
      // edge(<minus-2-arg1>, <minus-2>, "-}>"),
      // edge(<minus-2-arg2>, <minus-2>, "-}>"),    
    )
  },
  caption: [The compute graph generated for output cell `D5` from the formulae and structures of @fig:overview:structure-representation. Notice the stark difference in representation and simplicity with @fig:family-budget:income-summary-compute-graph where we do not have structures.],
)<fig:overview:compute-representation>

Besides the computations, we also extract the data into the _Data Model_. The _Data Model_ represents the data and shape of the structures and constants in the spreadsheet. Here, we rely on the type inference being done in the computation phase and the found structures of the structure phase. This results in the following representation

[Insert Data Representation here]

The penultimate step is converting the previously mentioned computational model and data model to the code layout model. In this step, we leave behind the structural compilation, and fully focus on the code. In this last step, we implement a few compiler passes that simplify the code or increase the readability by splitting expressions.

[Insert Code Representation here]

Finally, the code is converted to the data model of the Roslyn Compiler Platform. This platform allows for programmatically emitting C\# source code.

Now that we have given a grand overview, we will dive deeper into the different phases of the compiler. We first discuss the structural phase, pointing out the similarities between the excel spreadsheet and the structural representation. Then, we move over to the compute phase, formally describing the compute model, both in the compute graph and compute grid. We also cover the data phase and representation and discuss how we extract information from this. Finally, we combine the previous two and describe the code representation and the code phase.

= Modeling the workbook

As we have discussed in @sec:trivial-compiler, the structure is of utmost importance for readable code. Hence, it is important to model the structure of the excel workbook. In the first phase of the compiler, the _structural phase_, exactly does this. To capture the design, we use the _structural model_, an intermediate representation designed specifically for the spreadsheet workbook. In this section, we discuss this phase and the accompanying representation along with the passes needed to construct it fully.

== Reading the Excel File
An Excel file contains all the information about the workbook and the spreadsheets within. Essentially, it is an archive with individual files describing the workbook or spreadsheets, depending on the Excel versions. Excel files for older versions (before Excel 2007) were much harder to read since the workbook data is encoded. Since Excel 2007, a new excel file structure was introduced: the _.xlsx_ file. This file uses XML documents to describe the workbooks and their data and is much easier readable. In this thesis, we only focus on `.xlsx` files.

#figure(
  ```
xl/
│   calcChain.xml
│   connections.xml
│   sharedStrings.xml
│   styles.xml
├── workbook.xml
├── drawings
│   └── drawing1.xml
├── media
│   ├── image1.png
│   └── image2.svg
│
├── model
│   └── item.data
│
├───tables
│   └── table1.xml
│       table2.xml
│
├───theme
│   └── theme1.xml
│
└───worksheets
    ├── sheet1.xml
    ├── sheet2.xml
    └── sheet3.xml
  ```,
  caption: [The structure of an `.xslx` file. The tree has been slightly edited for improved legibility.],
  placement: auto,
)<fig:excel:structure>

The `.xlsx` archive contains many xml files storing computations, such as the last-calculated cells in the `calcChain.xml` or an overview of all strings in `sharedStrings.xml`. An overview of the excel structure can be seen in @fig:excel:structure. [Provide an in-depth example of what the excel file is].

For our own intermediate represenation we only consider the worksheets in the `sheet{N}.xml` files. Within these files, the grid is saved in a two-dimensional structure like can be seen in @fig:excel:sheet-structure. For every cell the value on display is stored. For cells computed by formulas, the formula is also stored. We can use this information to create the basis of our model.

#figure(
```xml
        <row r="15" spans="1:12" ht="30" customHeight="1" x14ac:dyDescent="0.45">
            <c r="A15" s="10"/>
            <c r="B15" s="14"/>
            <c r="C15" s="38" t="s">
                <v>7</v>
            </c>
            <c r="D15" s="26">
                <v>6000</v>
            </c>
            <c r="E15" s="26">
                <v>5800</v>
            </c>
            <c r="F15" s="27">
                <f t="shared" ref="F15" si="0">E15-D15</f>
                <v>-200</v>
            </c>
            <c r="G15" s="14"/>
            <c r="H15" s="14"/>
            <c r="I15" s="14"/>
            <c r="J15" s="14"/>
            <c r="K15" s="14"/>
            <c r="L15" s="14"/>
        </row>
```,
  caption: [A row in the `sheet1.xml` file describing the contents of the cells in the worksheet.],
  placement: auto
)<fig:excel:sheet-structure>

=== Extracting the information
In order to work with the information, we extract the information to our own two-dimensional spreadsheet model. We look at the `sheet.xml` files and parse them to convert the data to our model. When we parse the files, we distinguish between two cells: a _value cell_ and a _formula cell_. The value cell contains constant data and is copied directly without further modification. The formula cell contain a composition of Excel formulae and are parsed before copied to the model. 

The spreadsheet is converted in it's whole, and we might parse sections that we will not use in subsequent steps of the compilation. This is mainly due to the fact that we don't do a dependency analysis in this phase of the compiler.

In order to parse the formula, we use the work of #citeauthor(<aivaloglou_grammar_2015>) who created a near-perfect parser for Excel formulae.  Their "XLParser" tool converts the formula string to a parse tree. We then transform the parse tree to our own abstract syntax tree consisting of basic operations as can be seen in the definition of the tree in @fig:excel:formula-syntax.

#figure(
  "[[Placeholder]]",
  caption: [The definition of our Formula syntax language. ]
)<fig:excel:formula-syntax>

- Extract the whole spreadsheet
- Converting formulas

This forms the basis of the _structure model_, which is essentially a two-dimensional grid of cells which either hold data or formulae. On top of that, the structure model also models potential areas for structures and the implicit structures. 

---

- Provide an in-depth example of what the excel file is, and how we read it to our own intermediate representation.
- Allow a quick mention of the EPPlus library, which we are using for this research.
- Mention the XLParser library and the research paper behind it.
- Give an non-formal introduction of the structure IR.
- Use the budget spreadsheet as an example.

== Extracting Areas & Implicit Structures

In @sec:trivial-compiler we saw that we can use implicit structures in the excel sheet to optimize compilation. One of the major problems with these structures is that they are implicit, meaning it is hard to detect them within the spreadsheet. In two separate compiler passes from extracting the spreadsheet, the Excelerate compiler uses a three-step model for detecting and constructing these structures:
+ Detecting _Areas_,
+ Assessing if the area meet the criteria for structures,
+ Extracting the structures.

Step one is done in a separate compiler pass, and step two and three are also being done in a spearat compiler pass. Step two and three could have been one step, but splitting them up provides for a cleaner spearation of concerns and thus cleaner code and better maintainability of the compiler. 

=== Detecting Areas
An area is a continuous region in a spreadsheet that is marked as a potential structure. The structures we described above are all continuous areas.

The detection of areas is done with a simple neighbour detection algorithm. The spreadsheet is converted to a adjecency graph, with nodes representing cells and edges representing the neighbour relation. Thus, if two nodes are connected, they are neighbours in the spreadsheet. 

Detection of areas is done by detecting all connected components in this adjecency graph. When a connected component is detected, it is converted to an area by taking the most-top-left node and the most-bottom-right node and construct a rectangle within the spreadsheet. This means that when there is a 'gap' in the area, it will still get detected.

#let calc-column = super(text([*_f_*(x)], size: 1em, fill: luma(30%)))

#figure(
  spreadsheet(
    columns: 9,
    [], [], [], [], [], [], [], [], [],
    [], [], [*Montly Expenses*], [], [], [], [], [], [],
    [], [], [], [], [], [], [], [], [],
    [], [], [*Description*], [*Category*], [*Projected*], [*Actual*], [*Difference* #calc-column], [], [],
    [], [], [Extra... activities], [Children], [40.00], [40.00], [=D2-C2], [], [],
    [], [], [Medical], [Children], [], [], [=D3-C3], [], [],
    [], [], [School supplies], [Children], [], [], [=D4-C4], [], [],
    [], [], [School tuition], [Children], [100.00], [100.00], [=D5-C5], [], [],
    [], [], [Concerts], [Entertainment], [50.00], [40.00], [=D6-C6], [], [],
    [], [], [Live theater], [Entertainment], [200.00], [150.00], [=D7-C7], [], [],
    [], [], [Movies], [Entertainment], [50.00], [28.00], [=D8-C8], [], [],
    [], [], [Music ], [Entertainment], [50.00], [30.00], [=D9-C9], [], [],
    [], [], [], [], [], [], [], [], [],
  ),
  caption: [An excerpt of the Montly expenses spreadsheet of the family budget workbook.],
  supplement: "Spreadsheet",
  placement: auto
)<sps:monthly-expenses>

A gap in the data is quite common when there is data missing. That said, the sheet can still be calculated because Excel uses default or identity values like 0 when it encounters an empty cell. For instance, in the Montly Expenses in @sps:monthly-expenses, a gap can be seen in the range E6:F7. We can still calculate the computed column since Excel just calculates `0-0=0`. Thus, we should also include these empty cells in the area. Luckily, since we construct the area with the most-top-left and most-bottom-right cells, these gaps will still be detected.

Furthermore, we can see that @sps:monthly-expenses contains two areas. The first area is the table in the range C4:G12, containing the gap discussed above. This area has correctly targeted the area that contains the table. The other area is the cell C2. This single cell is captured as an area according to the algorithm, but it is clear that a single cell cannot form a structure. Hence, we exclude areas that are smaller than 2 rows.

A side-effect of this algorithm is that the title of the table ("Montly Expenses") in C2 will now be dropped and will not be included in the detected area. This is a loss of information since we will not use it further in the compiler. To combat this, other area detection algorithms can be used, or the adjecency graph can be adapted to also include information about the background color of the cells, but that is outside the scope of this thesis. For instance, if the background is the same color, it can also be regarded as the same area. However, this would increase the false positives when the whole spreadsheet is a single colour.

==== Algorithm for finding areas
We discussed the high-level overview of finding the areas. In @alg:areas the pseudo code for this algorithm can be found, which explores the algorithm in much more detail. The algorithm has three steps:
1. Convert the spreadsheet to a graph. Initialize an array of nodes from all the spreadsheet locations and for every node, find the neighbours in the list by comparing their location. Every node contains the location of one cell.
2. Find the connected components using the Hopcroft-Tarjan DFS algorithm @hopcroft_efficient_1973. It outputs a set of connected nodes.
3. Create the areas. For every connected component, get the top left most and bottom right most nodes and construct a new area from this.

#figure(
  text(raw("
    List<Area> Detect(Spreadsheet ss)
    {
        g  = ToGraph(ss);
        cc = HopcroftTarjan(g);
        a  = CreateAreas(spreadsheet, connectedComponents);

        return areas;
    }

    List<Node> ToGraph(Spreadsheet spreadsheet)
    {
        // Convert the spreadsheet to a graph
        Node[] nodes = Init(spreadsheet);

        foreach (var node in nodes) 
        {
          // Get the neighbours by comparing the locations of other nodes in the list.
            foreach(var neighbour in Neighbours(node)) {
              node.Neighbours.Add(neighbour);
            }
        }

        List<Node> graph = nodes.Values.ToList();
        return graph;
    }
    
    List<Area> CreateAreas(Spreadsheet spreadsheet, List<HashSet<Node>> connectedComponents)
    {
        List<Area> areas = new();
        foreach (var component in connectedComponents)
        {   
            // Create the range
            Location left = TopLeftMost(component);
            Location right = BottomRightMost(component);
            Range range = new Range(left, right);
            
            var area = new Area(range);
            areas.Add(area);
        }

        return areas;
    }
  ", align: left, block: true, lang: "cs", tab-size: 2)),
  caption: [Algorithm for finding the areas within a spreadsheet.],
  placement: auto,
)<alg:areas>

=== Identifying Structures
Once we have identified the areas, we should identify and extract the structures out of the spreadsheet. Like we discussed in the introduction to this section, we still have two remaining steps to do this. First, we identify the area by looking if an area meets the requirements for a special structure, then we extract the structures from the areas that meet expectations. This is done in a sequential matter since many of the structures have overlapping criteria. For instance, a chain can also be a table without having any recursive columns. In this section, we will detail the algorithms and criteria used for detecting and categorizing different structures.

The algorithm for detecting and categorizing is pretty simple. For every area, we sequentially check whether it meets the requirements for a specific structure. When it meets those requirements, we classify the area as that structure. The requirements are different for every structure. 

==== Table
The Table structure as described in @subsec:structures:tables can be detected according to its characteristics. To reiterate, a table is a rectangular data structure with optional title, headers and footer. The rows of the table do not interact with eachother, only intra-references are allowed. 

The tables are detected according to the following specification and requirements.

+ A table MAY have a title. If the table has a title, it MUST be in a single cell in the top row and the rest of the row MUST be empty.
+ A table MAY have a header. If the table has a title, the header MUST be in the second row. If the table does not have a title, the header MUST be in the first row. The header row MUST only contain string value cells.
+ A table MUST have more than one data row.
+ A data table column MUST have cells of the same type excluding empty cells.
+ A computed table column MUST only use references from the same row. All cells in the computed column MUST be the same formula, excluding the differences for row references. It should have the same 'shape'.
+ A table MAY have a footer. If the table has a footer, it MUST be the last row of the table. If the first column is a data column, the first cell of the footer MAY be a string value cell (for the "TOTAL" text or similar). The operations in the cells MUST be aggregation operations, excluding the first cell.

The area in @sps:monthly-expenses is a table. The table has a header, which is in the first row (since the table does not have a title) and all of them are strings. When the algorithm has detected title, headers and footers, it checks the suspected columns of the rows in-between the header and footer. In the case of the @sps:monthly-expenses, there are four data columns, since they all have the same type of cells. Notice that in the _Projected_ and _Actual_ columns, there are two empty cells. Even with those empty cells, we still recognize the columns as data columns.

#figure(
  spreadsheet(
    columns: 3,
    [Data Column 1], [Data Column 2], ['Computed' Column],
    [_a_], [_x_], [`= (4 * B2) + A2`],  
    [_b_], [_y_], [`= (4 * B3)`]),
  caption: [An example of a computed column with mismatching shapes.],
  supplement: "Spreadsheet",
  placement: auto
)<table:shape:example1>

To compare the formula cells in the computed columns, this thesis introduces the _shape_ of a formula. The _shape_ is the shape of the relative dependencies of the cell. With the shape of a cell, we can easily check if a cell has the same relative dependencies as other cells in the same column. For instance, the computed column in @table:shape:example1 has two cells with different shapes, since the relative dependencies are different: The `C4` cell misses the extra `+ A3` at the end.

[Insert representation of the table here (we really need an upgraded spreadsheet package)]

If all checks pass, the table will be converted to an actual table. The table will be converted into a range of selections. These selections represent a range on the worksheet and act as helper datastructures. We essentially 'mark' an area in the spreadsheet and say that area is the title, header, columns, or footer. It results in the _Table_ structure that is added to the workbook.

==== Chain
The chain-table as described in @subsec:structures:chain can also be detected according to its characteristics. The chain table looks a lot like a normal table but has the changed restriction that it allows for recursive columns, where the dependencies of the cells in that column may be from another row. 

Chains are detected according to the following specification and requirements:
+ A chain MAY have a title. If the chain has a title, it MUST be in a single cell in the top row and the rest of the row MUST be empty.
+ A chain MAY have a header. If the chain has a title, the header MUST be in the second row. If the chain does not have a title, the header MUST be in the first row. The header row MUST only contain string value cells.
+ A chain MUST have more than one data row.
+ A chain MUST have one or more initialisation rows.
+ A data chain column MUST have cells of the same type excluding empty cells.
+ A computed chain column MUST only use references from the same row. All cells in the computed column MUST be the same formula, excluding the differences for row references. It should have the same 'shape'. All cells in the computed column MUST not reference a cell from a calculated chain column.
+ A calculated chain column MUST have all cells use references from the same row or the rows above it. All cells in the chain column must have the same shape. 
+ A chain MAY have a footer. If the chain has a footer, it MUST be the last row of the chain. If the first column is a data column, the first cell of the footer MAY be a string value cell (for the "TOTAL" text or similar).

#figure(
  spreadsheet(
    columns: 6,
    [*Date*], [*_Interest_*], [*Deposit*], [*_Total_*], [], [_Interest Rate_],
    [01/03/2021], [], [], [10000], [], [0.015],
    [01/04/2021], [`= D2 * F1`], [500], [`= D2 + B3 + C3`], [], [],
    [01/05/2021], [`= D3 * F1`], [500], [`= D3 + B4 + C4`], [], [],
    [01/06/2021], [`= D4 * F1`], [500], [`= D4 + B5 + C5`], [], [],
    [01/07/2021], [`= D5 * F1`], [500], [`= D5 + B6 + C6`], [], [],
    [01/08/2021], [`= D6 * F1`], [500], [`= D6 + B7 + C7`], [], [],
    [01/09/2021], [`= D7 * F1`], [500], [`= D7 + B8 + C8`], [], [],
  
  ),
  caption: [An excerpt of the savings spreadsheet in the family budget workbook.],
  supplement: "Spreadsheet",
  placement: auto
)<chain:example1>

While the specification of the chain looks similar to the table, it is important to note that when a computed chain references a recursive chain column, it will automatically become an recursive chain column since we need the value of that cell, which can only be calculated recursively. Furthermore, a big difference between the table and the chain is the initialisation. We detect the initialisation by looking at the cell types. When a column needs an initialisation, it will need a value cell for the base case or initialisation and formula cells to calculate the rest.  

A great example of the conversion of a chain is the savings spreadsheet, for which an excerpt can be found in @chain:example1. Note that we find two areas: A1:D8 and F1:F2. Only A1:D8 is detected as a column. We can see two recursive columns: _Interest_ and _Total_ which both reference the previous columns. 

Extracting the chain structure is nearly the same as the table: we simply mark the initialisation and data columns separately in order for them to be categorized and transformed in the next phase. We mark these with the help of the `Selection` datastructure. It results in a _Chain_ structure that is added to the collection of structures in the workbook.

// // Once we have identified the areas, we should identify and extract the structures out of the spreadsheet. Like we discussed in the introduction to this section, we still have two remaining steps to do this. First, we identify the area by looking if an area meets the requirements for a special structure, then we extract the structures from the areas that meet expectations. 
// - Describe the algorithm for finding structures.
// - Describe the algorithm for detecting the table, chain and repository structures.
//   - Use the Expenses and Savings of the budget spreadsheet as an example.

// === Extracting Structures

  
== Formal definition of the Structure IR

We have discussed the structural phase of the compiler in high level above. Now, we want to take the time to cover a more formal definition of the structure model. This representation captures the exact essence of the whole Excel Workbook (or file) and allows for manipulation on cell level. The model preserves the grid layout of the spreadsheet and acts as a foundational structure for building higher-level representations, anchoring the subsequent computations to their visual and logical positions in the spreadsheet. An overview can be seen in @ir:structure.

#figure(
```cs
record Workbook(
    string Name, 
    Reference[] NamedReferences, 
    Spreadsheet[] Spreadsheets);

record Spreadsheet(string Name, Set<Cell> Cells, Structure[] Structures);

record Structure(string Name, Range Location);
  -> record Table(Selection header, Selection[] columns);
  -> record Chain(Selection header, Selection initialisation, Selection[] columns)

record Cell(Location Location);
  -> record ValueCell<T>(T Value);
  -> record FormulaCell(string Formula);

record Table(string Name, Range Location, Column[] Column);

record Reference();
  -> record Location(int Row, int Column, string Spreadsheet)
  -> record Range(Location From, Location To)
  -> record TableReference(string Table, string[] Columns)
```,
placement: auto,
caption: [The definition of the Structure Model. It essentially represents the Excel workbook while adding some features like the structures. ]
)<ir:structure>

=== Workbook
The workbook is the grand model of the Excel file, representing all the data. A workbook contains references to the spreadsheets that fill up the workbook. The workbook has a name that is typically copies the filename of the parsed Excel file.

=== Spreadsheet
The spreadsheet can be seen as a two-dimensional grid of cells. The compiler allows for infinitely large spreadsheets, but in practice Excel does not allow spreadsheets to have more than 1,048,576 rows and 16,384 columns #footnote([By Excel Specification: #link("https://support.microsoft.com/en-us/office/Excel-specifications-and-limits-1672b34d-7043-467e-8e27-269d656771c3")]). 

The spreadsheet also contains a name, typically derived from the name of the worksheet in Excel. Additionally, the spreadsheet contains the set of structures found in the spreadsheet.

=== Structures
A structure is a continuous region of cells in the spreadsheet that has a special meaning. We distinguish between two structures as we have discussed above: _Tables_ and _Chain-tables_. Both structures have their own structure and represent something different.

==== Table
The table contains references to ranges on the spreadsheet to the title, header, columns and footer. The columns are partially parsed with their corresponding headers. If there are no headers present, the columns get headers in the form of `Column{i}` where `i` is a counter from 0 to the number of columns. 

==== Chain-table
The chain table is similar to the table, but an extra reference is added. As such, the chain table contains references to the ranges on the spreadsheet of the title, header, initialization vector, columns and footer. 

=== Cells
The cell is the atomic unit in a spreadsheet. It represents a constant value or computation depending on the content. A cell is represented by a location and a value. Depending on the value, we assign a special status to the cell. If the content is a raw value like numbers or text, we say that the cell is a _value cell_. If the content is a formula expression that starts with `=`, we call the cell a _formula cell_.

==== Value Cells
Value Cells 

==== Formula Cells
Formula cells are special cells that contain formula and form the basis of the computational model and the transformation of the structural model to the compute model. 

The formulas get their formulas parsed in order to help the detection process of the structures. 

==== Formatting
It is possible to use custom formatting for a cell in Excel. For instance, a number $4.00$
can be formatted to let it look like a currency $euro 4.00$. It is often used with dates. 

We do not consider value formatting primarily because Excel stores the content of a cell apart from the formatted value. Hence, we can always take the 'normalized' value and use these in our calculations. This avoids the conversion of formatted values to normalized values.

That said, the color and fill of the cell can be valuable information when detecting structures, since structures often have the same color in a well-formed spreadsheet. 

=== References
An important construct we have not talked about is the reference. References are a way to refer to a cell, range or table. They link computations together by representing the (computed) value in another computation. While we consider them to be part of the structure model, they acutally also get used in 

// One way to look at a reference is in a _singular_ form. This means that the reference B3 is essentially the same as B3:B3, which denotes a range from cell B3 to cell B3. Semantically, they are the same as they both point to the same cell. 

We distinguish between references to cells like `B3`: _Cell References_; references to ranges like `A1:B3`: _Range References_, and references to tables like `Table[ColumnName]`: _Table References_. We distinguish between these references since they contain information that we can use in further compilation. Take the cell B3 for instance. This cell reference is essentially the same as B3:B3 as they both point to the same cell. However, the B3 cell reference is conceptually different than B3:B3 in programming terms, as the cell reference is just a single value, and the B3:B3 is a singleton array.

==== Data References
Cell and Range references are references that reference values and are addressing the structure of the spreadsheet. They cannot reference data that might not be in the spreadsheet yet. For instance, in the budget example, the list of expenses might be different from month to month. To support this, the Excel compiler considers this data (especially in tables and chains at the moment) separately (which we talk about more in @sec:data-model). To reference this data, we use a data reference.

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