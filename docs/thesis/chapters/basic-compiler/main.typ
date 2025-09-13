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
  show raw.where(block: false): r => {
    set text(font: "JetBrains Mono", size: 1em)
    r
  }
  
  it
}


#chapter("Compiling Excel")

// Sections

The first step in creating human readable code and answering the research questions is to convert the Excel sheet to executable code. Spreadsheets are executable programs at their core. The web of formulae, linked through dependencies forms the execution model of the spreadsheet. To translate this model into human-readable, verifiable, and reusable code, we first need a mechanism that can faithfully compile an Excel workbook into a general-purpose language.

// At first glance, an Excel workbook might seem like a collection of mere numbers and formulae, arranged in rows and columns. Yet, beneath this seemingly simple grid lies a carefully orchestrated structure that not only conveys data but also encodes semantics and context.

This chapter introduces such a mechanism: the 'basic' Excel compiler. This straightforward compiler sole task is to transform any spreadsheet into executable C\# code. As we will see, we require three separate phases for this: extracting relevant data in the _Structure Phase_; constructing a computational plan in the _Compute Phase_; and emitting semantic-preserving C\# code in the _Code Phase_.

We start with a high level overview of the compiler, briefly touching upon each phase. Then, we dive deeper into each phase: discussing the intermediate representations used, and the transformation step needed to compile Excel. Finally, we demonstrate the compiler at the end of the chapter, discussing its shortcomings and setting the stage for a more advanced technique which will be the focus of the following chapter.

#place.flush()

= High Level Overview<sec:hlo>

#figure(
  // [placeholder],
  image("sketch-basic-compiler-flow.png"),
 caption: [The flow of the basic compiler. The content of the Excel workbook is extracted and parsed into the _Structure Model_ (Workbook). Then this workbook is further analysed to find the underlying compute model and is transformed into a compute grid. The disconnected cells are then connected through their formula dependencies into a compute graph. Finally, a generic code layout model is created and C\# code is emitted through the Roslyn API.],
 placement: auto,
)<fig:high-level-overview>

#figure(
  spreadsheet(
    columns: 4,
    [], [*Projected*], [*Actual*], [*Difference*],
    [Income 1], [6.000], [5.800], [`=C2-B2`],
    [Income 2], [1.000], [2.300], [`=C3-B3`],
    [Extra Income], [2.500], [1.500], [`=C4-B4`],
    [TOTAL], [], [], [`=SUM(D2:D4)`]
  ),
  caption: [A extract of the 'Family monthly budget' sheet in the 'Family monthly budget'. This spreadsheet calculates the differences in projected and actual income.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:hlo:spreadsheet1>

Compiling an Excel Spreadsheet to Csharp code is done in several steps. Before we dive deeper into the different compiler steps shown in @fig:high-level-overview, we first give a high level overview of the compiler in order to get a feel for what is coming. We begin with a `.xlsx` spreadsheet file, the standard file format for Excel files. As an example, consider @sps:hlo:spreadsheet1, a small spreadsheet that has been saved as `spreadsheet.xlsx`. This spreadsheet will be compiled into a program using the compiler. We will use cell `D5` as an output cell, meaning that the compiled code should output the sum of the differences.

In the first phase of the compiler, the _Structural Phase_, we extract the _Structural Model_ or _Workbook_ out of the provided spreadsheet. This is comparable with getting an AST of a code file: we get an intermediate language that models the spreadsheet and allows for easy manipulation and analysis. As such, we extract specific information from the whole Excel file: cells, formulae, tables. This is done through reading the XML files embedded in the `spreadsheet.xlsx` and parsing them to an internal model. 

#include "figures/high-level-overview-compute-model.typ"

In the _Compute Phase_, the next phase of the compiler, we construct a _Compute Graph_. This graph models the computational flow in the Excel file. Based on the provided output---in this case `D5`---we analyse the formulae in the provided cell and convert it to the _Compute Model_. The dependencies of the formula are located using the _Structural Model_ and recursively converted to this new model. The result is a graph of formulae that feed into each other and ultimately compute the value of the output cell. For @sps:hlo:spreadsheet1 the result can be found in @fig:high-level-overview:compute-graph.

Then we enter the _Code Phase_ of the compiler. This phase converts the _Compute Graph_ created in the previous phase to the _Code Model_. This intermediate representation is specifically created for easy manipulation and is meant to be language agnostic, supporting both functional as imperative paradigms. In this phase, we inline variables and transform the code to be more in line with C\#'s imperative nature. Both these compiler passes have the purpose of simplifying the code.

Lastly, the data in the _Code Model_ is transformed using the C\# compiler platform API (Roslyn) to C\# code classes. The class library is created and is now ready for use in external scripts. 
In @code:hlo:spreadsheet1, the final compiled version of @sps:hlo:spreadsheet1 can be seen that was compiled using this 'basic' Excel compiler.

#figure(
  ```cs
  public class Program
  {
      public double Main()
      {
          double sheet1D2 = 5800 - 6000;
          double sheet1D3 = 2300 - 1000;
          double sheet1D4 = 1500 - 2500;
          double sheet1D5 = new List<double> { sheet1D2, sheet1D3, sheet1D4 }.Sum();
          return sheet1D5;
      }
  }
  ```,
  caption: [A compiled version of @sps:hlo:spreadsheet1 with the 'basic' Excel compiler described in @sec:hlo. The compiler emits a method that calculates the value of the `D5` cell.],
  placement: auto,
)<code:hlo:spreadsheet1>

Now that we have given a grand overview, we will dive deeper into the different phases of the compiler. We first discuss the _Structural Phase_, extracting the spreadsheet data. Then, we move over to the _Compute Phase_, formally describing the compute model in the form of the _Compute Graph_. Finally, we explain the _Code Phase_ and the step from the _Code Model_ to the actual C\# code.

= Extracting the Data<sec:structural-phase>

In order to analyse the Excel file and its spreadsheets, we need to parse it to our internal data structure. In this section, we introduce the _Structural Model_: an internal data structure mimics the objects found in Excel we covered in @sec:intro:excel. The _Structural Phase_ contains the parsing of an Excel file to the _Structural Model_. In the next chapter, we will expand this phase to detect structures in the spreadsheet and enhance the _Structural Model_.

The _Structural Model_ only parses the Excel file and as such does not work with the input and output the user has defined. This has two reasons. The primary reason is that we need the AST of the spreadsheet to analyse in which area we are. Besides, we could prune certain 'unused' sections of the spreadsheet while creating the AST, but makes this phase unnecessary complex. For simplicity, we load the entire spreadsheet and prune unused parts in the next phases.

In this section, we will first discuss what the _Structural Model_ entails and introduce the _Formula Model_: a simplified AST for Excel formulae. Then, we discuss what an Excel file contains and how the _Structural Model_ is extracted from the Excel Worksheet.

== Structural Model<subsec:structural-model>

The _Structural Model_ is a data structure that captures the contents of an Excel file and allows for manipulation and analysis on cell level. The model preserves the tabular format of Excel spreadsheets.
@ir:structural-model describes the formal definition of the _Structural Model_.

#figure(
  ```cs
  record Workbook(string Name, Table[] Tables, Spreadsheet[] Spreadsheets);
  
  record Spreadsheet(string Name, Set<Cell> Cells);
  
  record Cell(Location Location);
    -> record EmtpyCell();
    -> record ValueCell<T>(T Value);
    -> record FormulaCell(Formula Formula);
  
  record Table(string Name, Range Location, string[] Columns);
  ```,
  placement: top,
  caption: [The formal definition of the Structure Model. It essentially represents the Excel workbook. A record represents a node in the AST. Every record has properties. The AST is built-up from the Typed properties in records: if one record has another record as property, they are linked in the AST. ]
)<ir:structural-model>

=== Workbook
The _Workbook_ serves as the top-level model of the Excel file. As such, it is the top node of our AST. It encapsulates all the data and structure. A workbook contains references to one or more _Spreadsheets_. Additionally, _Excel Tables_ that are created in _Spreadsheets_ are also stored at this level. This is to accommodate simpler look-up for tables, which can be referenced from any _Spreadsheet_ in the _Workbook_. Furthermore, a workbook has an associated name, which usually mirrors the filename of the original Excel file.

=== Spreadsheet
The _spreadsheet_ is derived from the _Excel Worksheet_ (See @sec:intro:excel): it is sparse representation of a two-dimensional grid of cells. We only store the non-empty cells. When a location is not found within the data structure, we return an _Empty Cell_. Additionally, the _Spreadsheet_ is named in order to support inter-_Spreadsheet_ references (see @sec:intro:excel).

=== Cell
The _Cell_ is the atomic unit in a spreadsheet. A cell is represented by a location and a value. Depending on the content of the value, it represents a constant value or computation. If the content is a raw value like numbers or text, we say that the cell is a _Value Cell_. If the content is a formula expression that starts with `=`, we call the cell a _Formula Cell_.

==== Value Cell
The _Value Cell_ is a cell that contains a raw value. A _Value Cell_ will always have a type that can be determined at the time we read the value. For instance, cell `B1` in @sps:hlo:spreadsheet1 is a _String Value Cell_, while C3 is a _Numeric Value Cell_. Excel stores all numbers in the same generic numeric format. To stay consistent, the compiler also does not distinguish between different numerical types.

The values of cells in Excel can be formatted to provide extra visual context for the Excel user. For instance, a number $4.00$
can be formatted to let it look like a currency $euro 4.00$. It is often used with dates. Within the _Structural Model_, we do not consider value formatting since it does not alter the calculations. Furthermore, Excel stores the original value of a cell apart from the formatted value, and as such, we can always use this normalised value.

==== Formula Cell
A _Formula Cell_ is a cell that contains a _Formula_. Just like we discussed for Excel in @sec:intro:excel, the _Formula_ calculates a value using dependencies and functions available in the _Workbook_. The _Formula_ is immediately parsed to the _Formula Model_ in order to access the AST. This also allows for analysis in the next phase. We discuss the _Formula Model_ in the next subsection.

=== Table
All _Excel Tables_, as described in @sec:intro:excel, are mapped to a _Table_. A _Table_ is a sub-section of the grid in the _Spreadsheet_ describing data in a tabular format. Like _Excel Tables_, _Tables_ cannot overlap, i.e. two _Tables_ cannot share columns. _Tables_ have a name and defined columns. The columns may have a header giving them a descriptive name, such as 'Description' and 'Category' in the 'Monthly Family Budget' _Excel Worksheets_.

=== References
An important construct we have not talked about is the reference. References are a way to refer to a _Cell_ or a _Range_. They link cells together by representing the (computed) value in another cell or range. 

#figure(
  ```cs
  struct Reference();
    -> struct Location(int Row, int Column, string Spreadsheet)
    -> struct Range(Location From, Location To)
    -> struct TableReference(string Table, string[] Columns)
  ```,
  placement: auto,
  caption: [The formal definition of the References. These models are used throughout the compiler and thus stand apart from the _Structural Model_. A record represents a node in the AST. Every record has properties, which can be other AST nodes (record properties) or metadata (struct properties). ]
)<ir:references>

While we discuss them here, we consider them as their own. They are also utilised in other phases of the compiler to refer to the original location on the spreadsheet.

// One way to look at a reference is in a _singular_ form. This means that the reference B3 is essentially the same as B3:B3, which denotes a range from cell B3 to cell B3. Semantically, they are the same as they both point to the same cell. 

We distinguish between:
- _Location_: references to cells like `B3`; 
- _Range_: references to a continuous selection of cells in the grid of the _Spreadsheet_, like `A1:B3`;
- _Table Reference_: references to a column in a table like `Table[ColumnName]`. 

We distinguish between these references since they contain information that we can use in the next phases. Take the cell B3 for instance. This cell reference is essentially the same as B3:B3 as they both point to the same cell. However, the B3 cell reference is conceptually different than B3:B3 in programming terms, as the cell reference is just a single value, and the B3:B3 is a singleton array.

#figure(
  ```cs
  record FormulaExpression();
    -> record Function(string name, FormulaExpression[] arguments)
    -> record Constant(object value)
    -> record Reference()
      -> record Cell(Location location)
      -> record Range(References.Range range)
      -> record Table(TableReference tableReferences)
  ```,
  placement: auto,
  caption: [The formal definition of the Formula Model. It models the Excel Formula Language. A record represents a node in the AST. Every record has properties, which can be other AST nodes (record properties) or metadata (struct properties). Since there is a duplicate name 'Range', we added a namespace to the external entities. 'References.Range' refers to the _Range_ discussed in the previous section.]
)<ir:formula-model>

== Formula Model<subsec:structural-model:formula>

The _Formula Model_ models the Excel Formula language. It is a simple DSL that allows us to capture the functions, constant and references used in the formulae in a cell. The _Formula Model_ is a Intermediate Representation  In this subsection, we briefly discuss the formula model.
As we will cover in @sec:compute-phase, the Formula Model was the inspiration for the _Compute Model_. As such, many elements will look the same. The formal definition can be found in @ir:formula-model

All entities we cover in the next sections are _Formula Expressions_. A formula is always one big _Formula Expression_ that can be composed of several sub-expression. For instance, the '`=SUM(A1:B3)`' can be converted to the `Function("SUM", Range(A1:B3))` _Formula Expression_, which in turn contains the `Range(A1:B3)` sub-expression.

=== Function
The _Function_ represents a named procedure with optional parameters. It takes other _Formula Expressions_ as input and computes a value as output.

==== Operator
An _Operator_ is a special function. Just like a function it has a name and takes parameters to compute a value. However, it is fixed between operands. A common example of an operator is the `+` operator, which is placed between two operands like `1+1`

=== Constant
A _Constant_ is a value of any type that is used as dependencies in other _Formula Expressions_.

=== Reference
The _Reference_ uses the references discussed in the previous section to reference values calculated by _formulae_ in other cells. We consider three self-explanatory types: _Cell References_, _Range References_ and _Table References_. 

---

With the _Structural Model_ and _Formula Model_ defined, we have made it possible to describe full Excel files. Within this phase we do not have any other compiler steps. In the next chapter, we will analyse the whole _Worksheet_ for so-called _Structures_. These _Structures_ enrich the _Structural Model_ and go beyond plainly copying the Excel sheet.

Before we move to the _Compute Phase_, we first want to discuss how we managed to convert the Excel Workbook to the _Structure Model_.

== Constructing the model
Like we discussed, an Excel File contains all information about the workbook and the spreadsheets within. In the previous sections, we already discussed what we extract and what the importance of the extracted content was. That said, we did not cover how we obtain this information and how we actually construct the model. In this subsection, we clarify this omission. 

=== Structure of an Excel File
An Excel file is essentially an archive with individual files describing the _Excel Workbook_. Older versions of Excel (before Excel 2007) create `.xls` files that were harder to read due to the encoded nature of the files. Luckily, the newer `.xlsx` format is an archive consisting of XML files that is easily readable. These files use the `SpreadsheetML` spreadsheet dialect @microsoft_structure_2025 to describe the workbook.

Due to the encoded nature of the older Excel files, the proposed compiler only supports `.xlsx` files. These files are easier to read and convert to the _Structure Model_. However, since the _Structure Model_ describes an abstract version of the spreadsheet, future work could include a parser for `.xls` files.

#figure(
  ```
  xl/
  ├── calcChain.xml
  ├── connections.xml
  ├── sharedStrings.xml
  ├── styles.xml
  ├── workbook.xml
  ├──/tables
  │   ├── table1.xml
  │   └── table2.xml
  └──/worksheets
      ├── sheet1.xml
      ├── sheet2.xml
      └── sheet3.xml
  ```,
  caption: [The structure of an `.xslx` file. The tree has been slightly edited to improve legibility: folders like media, drawings and printer settings, as well as other XML configuration files have been removed.],
  placement: auto,
)<fig:excel:file-structure>

The `.xlsx` archive presented in @fig:excel:file-structure provides an overview of what can be found in an Excel file. Many files are not used, such as: 
- `connections.xml` storing connections to outside sources;
- `sharedStrings.xml` storing all unique strings in the document, which is used for space preserving;
- `styles.xml` containing all styles in the workbook.
We also do not use `calcChain.xml`, which contains the order of operations for calculating all cells with formulae in a workbook. This may be counterintuitive, since it could be valuable information for compilation. However, as we will see in the next chapter, we use optimisations that change the order of operations. 

We will use `workbook.xml`, which references the XML files in the `tables` and `worksheets` directories. Ultimately, these files build up the _Structure Model_. The `workbook.xml` contains metadata like the name of the workbook. In the `tables` directory, we find the files that describe the tables. Such a file contains the names and columns of a _Table_.

#figure(
```xml
<worksheet>
  <sheetPr codeName="Sheet1">
    <tabColor theme="9"/>
  </sheetPr>
  <dimension ref="A1:M25"/>
  <cols>
    ...
  </cols>
  <sheetData>
    ...
    <row r="17" spans="1:13" ht="30" customHeight="1" x14ac:dyDescent="0.45">
      <c r="A17" s="10"/>
      <c r="B17" s="14"/>
      <c r="C17" s="38" t="s">
        <v>52</v>
      </c>
      <c r="D17" s="26">
        <v>2500</v>
      </c>
      <c r="E17" s="26">
        <v>1500</v>
      </c>
      <c r="F17" s="27">
        <f>E17-D17</f>
        <v>-1000</v>
      </c>
      <c r="G17" s="14"/>
      <c r="H17" s="14"/>
      <c r="I17" s="14"/>
      <c r="J17" s="14"/>
      <c r="K17" s="14"/>
      <c r="L17" s="14"/>
    </row>
    <row r="18" spans="1:13" ht="25" customHeight="1" x14ac:dyDescent="0.45">
      <c r="A18" s="10"/>
      <c r="B18" s="14"/>
      <c r="C18" s="38"/>
      <c r="D18" s="26"/>
      <c r="E18" s="26"/>
      <c r="F18" s="27"/>
      <c r="G18" s="14"/>
      <c r="H18" s="14"/>
      <c r="I18" s="14"/>
      <c r="J18" s="14"/>
      <c r="K18" s="14"/>
      <c r="L18" s="14"/>
    </row>
    ...
</sheetData>
</worksheet>
```,
  caption: [An example of an XML file in the `worksheets` directory of @fig:excel:file-structure. This file stores the contents of the worksheet in `sheetData` as a matrix. The file has been altered for legibility, removing XML metadata and information about markup in the worksheet.],
  placement: auto
)<fig:excel:sheet-structure>

The real data resides in the `worksheets` directory. Here, every file contains the data of one of the _Worksheets_ that is part of the _Workbook_. Within this file lies a two-dimensional representation of the values and formulae of the Excel worksheet. Transforming these files is done by looping over all `<c>` elements in the XML file and converting them to _Cells_ in the _Spreadsheet_. The type of the cell is based on the existence of the `<f>` and `<v>` elements: 
- if both are present, we consider the cell to be a _Formula Cell_; 
- if only the `<v>` element is present, we consider the cell to be a _Value Cell_; 
- if both are omitted, it is an _Empty Cell_ and it will not be stored in the _Spreadsheet_.

Like we discussed in @subsec:structural-model:formula, a formula for a _Formula Cell_ needs to be parsed to their own DSL. We use the work of #citeauthor(<aivaloglou_grammar_2015>): their `XLParser` tool. This tool has a 99.9% successful parse rate and produces parse trees of the formula @aivaloglou_grammar_2015. This parse tree is converted to the _Formula Model_ and is used to construct the _Formula Cell_.

These two parsers create the _Formula Model_ and the _Structural Model_. Since the _Formula Model_ is part of the _Structural Model_---it is a sub-intermediate representation---constructing the model is done in one compiler step. Furthermore, since we do not have any extra compiler steps in this phase of the compiler, we can hand off the representation of the Excel Sheet, the _Structural Model_ to the next phase: the _Compute Phase_.

#pagebreak(weak: true)

= Deriving the Logic<sec:compute-phase>

Now that we have parsed the Excel file to an internal data structure called the _Structural Model_ in the previous section, we can start to analyse the _Workbook_ and begin deriving the logic hidden behind the cells. The real magic happens under the hood, where a network of formulas calculates the values of the formula cells. We want to extract this magic from the spreadsheet and translate it to actual code. This is what the _Compute Phase_ does: it extracts the computational model found in the network of linked formulas and cells and creates a dependency graph.

Like we discussed in the high level overview in @sec:hlo, the _Compute Phase_ utilises the _Compute Model_ to represent the underlying computational model as a _Compute Graph_. In this section, we cover the _Compute Model_, its relation to the _Compute Graph_, and a compiler step to enrich the _Compute Model_ even further. First, we introduce the _Compute Unit_, and its place in the _Compute Model_. Then, we introduce the _Compute Graph_ and how it is constructed from the _Structural Model_. Finally, we cover the type inference compiler step that enhances the model with type data needed for code compilation.

== Compute Model
The _Compute Model_ is a model of the underlying computational model. This intermediate representation provides a way to express computations and is used in different ways. In the 'basic' compiler, the _Compute Model_ is ultimately represented as a graph of interconnected nodes that compute the value in the desired output cell. In the next chapter, we expand upon this by introducing another way to represent this model.

At the core of the _Compute Model_ lies the _Compute Unit_. This unit represents a basic operation with input and output. _Compute Units_ can be connected to each other, forming a network or flow of computations. When the result of _Compute Unit_ _B_ is used as input of _Compute Unit_ _A_, we say that unit _B_ is a _dependency_ of unit _A_. Conversely, unit _A_ is a _dependent_ of unit _B_. 

#figure(
  ```cs
  record ComputeUnit(Type type, Location Location, ComputeUnit[] Dependencies);
    -> record Nil();
    -> record ConstantValue(object Value);
    -> record Input(string Name);
    -> record Function(string Name);
  ```,
  caption: [The Compute IR]
)<ir:simple-compute-model>

An overview of available _Compute Units_ can be seen in @ir:simple-compute-model.
_Compute Units_ have a few properties in common:
- A list of dependencies. This represents the _Compute Units_ this unit depends on. For some elements like the _Constant Value_, this list will always be empty since they do not depend on any value.
- A location in the spreadsheet. This location is used for traceability within the compiler so we know where this computation would have taken place in the spreadsheet. When extending the compiler, this can be of great help while debugging. Furthermore, we will use this location as a heuristic for a better code structure in the next section when we compile the _Compute Model_ to the _Code Model_.
- Every _Compute Unit_ has a type. This type represent the type of the value that the _Compute Unit_ outputs.



The _Compute Unit_, like an Excel Formula, represents a way of computing a value. As we foreshadowed in @subsec:structural-model:formula, the _Compute Model_ has a lot in common with the _Formula Model_ as it models the Excel Formula Language as well. The _Compute Model_ closely resembles the _Formula Expression_. However, there are a few differences. The first difference is the way the two models are constructed: the _Formula Model_ is constructed like a tree. In the _Compute Model_, graphs will form if there are multiple dependencies on one Compute Unit. Furthermore, the next chapter will extend the _Compute Model_, introducing new _Compute Units_ and essentially making the _Compute Model_ a superset of the _Formula Model_.

The individual _Compute Units_ that construct the _Compute Model_ can be directly derived from their _Formula Model_ counterpart. We discuss the similarities between the _Compute Units_ and _Formula Expressions_ since we cover the transformation between these nodes when we transform the _Structural Model_ to the _Compute Graph_ in the next section. 

==== Constant Value
A _Constant Value_ is a value of any type. The _Constant_ stores the data like a string (`'String'`) or numeric value (`42`) and directly relates to the constant in the _Formula Model_.  Often, the constant is the dependent of other _Compute Units_. However, it can also be an actual constant used within a formula, like the `2` in `=2*F3`, or a value that is extracted from a _Value Cell_. For instance, if `F3` in the previous example is a _Value Cell_, then its contents will be converted to a _Constant_.

==== Function
The _Function_ is a _Compute Unit_ that computes data from their dependencies, which are essentially the _arguments_ for the _Function_. The function is stored as its name and the arguments.

==== Nil
_Nil_ is a computation that computes nothing. We use _Nil_ to denote empty cells often found in ranges. In subsequent compiler steps, Nil is often converted to a default value or removed altogether. This is a new addition to the _Compute Model_ and is not represented by the _Formula Model_.

==== Input
Up until this point, we have not considered the input of the user yet. Within the _Compute Phase_, we introduce the _Input Compute Unit_ `In{N}`, which represents a value that is not known at compile time. The Input will always take the type of the cell it replaces.

We do not have any references in the _Compute Model_. This distinction from the _Formula Model_ comes from the fact that the _Compute Model_ links the dependencies to other cells. When all dependencies are removed, we get a web of interconnected _Compute Units_. We call this web the _Compute Graph_.

== Compute Graph

The next step in compilation is the conversion to the _Compute Graph_, linking the compute units in the individual cells to each-other, creating one big graph. The compute graph is a uni-directional non-cyclic graph that describes the underlying compute model of an Excel sheet. The _Compute Graph_ can be also seen as a _dependency graph_. The compute graph used in this thesis resembles the support graphs found in Excel or the open implementation by #citeauthor(<sestoft_spreadsheet_2006>), which also store computations as nodes and use dependencies to create a dependency graph.

Within the 'basic' compiler, the _Compute Graph_ fully represents the _Compute Model_. Unlike the previous phase, there are two compiler steps that enhance the _Compute Graph_ and therefore the _Compute Model_. In this subsection, we cover the conversion of the _Structure Model_ to the _Compute Graph_. Then, we discuss the two compiler steps: input insertion and type inference.

=== Model to Model
For the conversion of the _Structure Model_ to the _Compute Graph_ we look at the outputs the user has specified and convert that cell from the _Structure Model_ to a tree of _Compute Units_. This is recursive process, since the references of cell, and their references, etc, will be converted too. When a cell has been fully converted, it will be linked to its parent: it will become a dependency of the parent.

To be more precise, for every cell that we convert from the _Structure Model_ we distinguish between the type of the cell:
+ The cell is a _Value Cell_: the value and type of the cell is copied to a _Constant Value_ compute unit.
+ The cell is a _Formula Cell_: The formula is transformed from the _Formula Model_ to the _Compute Model_: Every _function_ and _operator_ is converted to a _Function Compute Unit_ and constant values are converted to _Constant Compute Unit_. When we encounter a _Reference_, we convert the cells in the Reference using the same method, recursively. When the cells are converted, they are added as a dependency to the dependent of the reference.

When a cell is converted, it is added to a dictionary that is checked before converting a new cell. When the cell that we want to convert is already converted to a _Compute Unit_, we use that _Compute Unit_. Else, we convert it using the above algorithm.

#figure(
  spreadsheet(
    columns: 2,
    [42], [`=2*A1`],
    [1.000], [`=1/B1 + A2`],
    [600], [`=SUM(B1:B2)`]
  ),
  caption: [A simple representation of the _Structure Model_ of a spreadsheet that contains several cells that are referenced by formulae in other cells.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:compute-phase:compute-graph-conversion>

Take the _Structure Model_ of a simple spreadsheet in @sps:compute-phase:compute-graph-conversion for example. If `B3` is considered an output, this is where the conversion starts. It tries to convert the _Formula Cell_ and converts it to `Function('SUM', [])`. It contains a _Range Dependency_ and as such, cells `B1` and `B2` also have to be converted. We convert `B1` (`=2*A1`) to `Function('*', [2, 42])` after converting `A1` to the _Constant Value_ `42`. If we want to convert `B2` (`=1/B1 + A2`) we need to convert `B1` again. However, it is present in the dictionary, so we use the already converted `B1`. `B2` is converted to `Function('+', [Function('/', [1, Function('*', [2, 42])]), 1000])`. Both `B1` and `B2` are then added as dependency to `B3`. 

------------------------------------ *[Create picture]*

An important side effect of this is that we only use the cells that are used in the calculation of the user specified output and ignore the rest. This makes this compilation step also an optimisation since we remove any cells that would be _dead code_.

=== Traversing the Compute Graph
The compute graph supports the operation for traversing the graph. This is done in topologically sorted way, which means that when traversing node _a_, we have already traversed the dependencies of _a_. This ensures consistent traversal and updating of the graph.
Traversing the graph and making updates to the graph is a common operation within a compiler step.

=== Input Substitution

The compiler takes cell references (_Locations_ like `'Sheet1'!A1`) as input that can be used as parameters for the generated code. These inputs need to be inserted into the graph. Utilising the graph traversal algorithm discussed in the previous section, we find the first Compute Unit with a location of one of the inputs. This _Compute Unit_ is replaced with an _Input Compute Unit_. Since the input compute unit represents a future value, it does not have any dependencies. As such, the dependencies of the _Compute Unit_ it replaced will be pruned unless they are referenced by other _Compute Units_ in the graph.

=== Type Resolution
In general, when compiling to strongly-typed languages, you need to know the types of the computations you want to model. We already saw in @sec:structural-phase that Excel provides the types at cell level: we used those to infer the types of the cells in a structure and could infer the type of a column based on that. The types we discovered in @sec:structural-phase do not cover the formulas. These types are automatically inferred by Excel and are not disclosed when parsing them. In other words, we need to resolve the types for individual compute units.

Since we do know the types of the leaves of the graph, as they are constant values and have been given a type from the start, we can infer the types of their dependents, and thus recursively build a typed Compute Graph. In order to know what the type is of a certain compute unit, we rely on the types of the dependencies and an inference rule. This inference rule describes what a valid inference is for this compute unit, and what its type might be if there is a valid inference. There can be multiple inference rules for one compute unit.

For instance, take the _Function_ compute unit, especially the `F('+')` compute unit, which is basic addition between two dependencies. See the inference rule in @inf:type-resolution:addition, which uses some syntactic sugar to state that the function `F('+')` can be type $tau$ if and only if it has two arguments: which is denoted by the pattern matching `[a1, a2]` that denotes that the list should have two elements `a1` and `a2`; and that those two arguments are both of the same type.

#import "@preview/curryst:0.5.1": rule, prooftree
#figure(
  prooftree(
    rule(
      [`F('+', [a1, a2])` : $tau$],
      [`a1` : $tau$],
      [`a2` : $tau$],
    )
  ),
  caption: [The inference rule for the `F('+')` compute unit, stating that it needs 2 arguments, and both arguments need to be of the same type.],
  supplement: "Rule"
)<inf:type-resolution:addition>

Some functions, such as `RAND()`, return a random number and does not take any inputs. These functions will always be one type. @inf:type-resolution:rand shows the inference rule for such a function. The same is true for _Constant Value_ compute units. These already have their types determined from the _Worksheet_ or from the parse tree of a _Formula_, as we discussed in previous sections.

#figure(
  prooftree(
    rule(
      [`F('RAND', [])` : `int`],
      [$tack.b$]
    )
  ),
  caption: [The inference rule for the `F('RAND')` compute unit. It is always an integer.],
  supplement: "Rule"
)<inf:type-resolution:rand>

==== Limitations
The compiler currently lacks support for Excel's `IF` function due to the thesis scope, but it is worth noting a limitation in the existing type-inference rules. In Excel, an if-statement is a normal function `IF(t1, t2, t3)` and thus maps to `F("IF", [t1, t2, t3])` in the _Compute Model_. A naive typing rule like @inf:type-resolution:if would require the two branch expression to share the same type, so that the outcome of the `IF` function also has this type.

#figure(
  prooftree(
    rule(
      [`F('IF', [d, b1, b2])` : $tau$],
      [`d`: `bool`],
      [`b1`: $tau$],
      [`b2`: $tau$]
    )
  ),
  caption: [The inference rule for the `F('RAND')` compute unit. It is always an integer.],
  supplement: "Rule"
)<inf:type-resolution:if>

#figure(
  spreadsheet(
    columns: 1,
    [`TRUE`],
    [`=IF(A1, 100, TRUE)`],
    [`=IF(A1, ISNUMBER(A2), A2)`]
  ),
  caption: [A simple representation of the _Structure Model_ of a spreadsheet that contains several cells that are referenced by formulae in other cells.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:compute-phase:type-resolution-limitation>

However, Excel allows these two branch expressions to return different types. The proposed inference system is unable to represent this, as the compute unit containing the if-statement would have more than one possible type. This calls for a type system with union types or type refinements based on the condition of the if-statement.

// Other inference rules, like for cell references are pretty straightforward: they just pass along the type of their single dependency. For range references, we do the same as for the plus operator: we check if the types are the same, and return a list of that type as the type for the range operator, see @inf:type-resolution:range

// #figure(
//   prooftree(
//     rule(
//       [`Range(args)` : $[tau]$],
//       [$forall arg in "args". arg: tau$]
//     )
//   ),
//   caption: [The inference rule for the _Range_ compute unit, which can have infinite elements. We require all elements to be of the same type.],
//   supplement: "Rule"
// )<inf:type-resolution:range>

= Generating Code

When we have finished transforming and optimising the _Compute Model_ and have created a representation of the computational model, it is finally time to produce actual code. The final phase of the compiler, the _Code Phase_ deals with this problem. During the code phase, the compute graph is converted to the code layout model, and then converted to actual programming languages. In this thesis, we transform the code to C\# code. 

The _Code Phase_ has two parts: (1) the language-agnostic part, where we convert the _Compute Model_ into the language-agnostic _Code Model_; (2) the language-specific part, where we compile the _Code Model_ into Csharp code. Most optimisations done in this phase are done on the language-agnostic _Code Model_. The _Code Model_ is a model that represents an abstraction of code. The _Code Model_ is built for easy manipulation and supports both object-oriented and functional features. Consequently, the _Code Model_ is able to model a lot of programming languages. The use of the _Code Model_ prepares the compiler for compilation to other languages than C\#, such as Java.

In this section, we will first introduce the _Code Model_ in more detail. We discuss the use of the _Let_ node to simplify manipulation and present further compiler steps possible for the language-agnostic _Code Model_. Then, we discuss the final part of compilation: the transformation from the _Code Model_ to C\#.

== Code Model
The _Code Model_ is the intermediate representation that provides structural guidance in emitting correct code and ease final transformation of the code. The model simplifies many parts of a normal parse tree and omits implied syntax. 

#figure(
  ```java
  Type(String name)
    -> Class(String name, Property[] members, Method[] methods)
    -> ListOf(Type member)
  
  Method(String name, Statement[] body)

  Property(String name, Type type, Expression? init, Expression? get, Expression? set);
  
  Statement()
    -> If(Expression cond, Statement[] then, Statement[] else)
    -> ExpressionStatement(Expression expression)
    -> Declaration(Variable variable, Expression value)
    -> Return(Expression expression);
  
  Expression(Type type)
    -> Assignment(Variable variable, Expression value)
    -> Constant(Type type, Object Value)
    -> FunctionCall(Expression? self, string name, Expression[] arguments)
    -> Lambda(Variable[] parameters, Expression body)
    -> Let(Assignment variable, Expression in)
    -> List()
    -> ListAccessor(Expression list, Expression index)
    -> MapAccessor(Expression map, Expression index)
    -> Property(Expression self, String name)
    -> Variable(String name)
    ```,
    caption: [A formal definition of the _Code Model_ IR.],
    placement: auto,
)<ir:code-model>

In comparison with the Roslyn compiler model (introduced in ...) the _Code Model_ is much simpler. The Roslyn compiler model is used to model the AST of .NET languages. It allows for complete modification of the syntax of the language. However, this can also result in invalid C\# syntax. For instance, C\# has a `var` type that automatically infers the type at compile time, like the `auto` type in C++. This 'type' can only be used as a direct or simple type, not as part of a complex or generic type, i.e. `var i = 0;` is valid code, but `List<var> = [1,2,3];` is not. With the Roslyn Compiler, it is possible to create such a type. Instead, the _Code Model_ strictly forbids this.

@ir:code-model presents an overview of the _Code Model_. Most of the nodes of this IR can be recognised from popular object-oriented and procedural programming languages, such as _Classes_, _Methods_ and _Function Calls_. As such, many elements will not be discussed. That said, some elements of the model, such as the _Let_ expression, require more elaboration.

=== Statements

The IR is able to model standard statements like variable declarations, return and if-statements. Those are synonymous with the statements in many popular languages. That said, an interesting node of the model is the _Expression Statement_. This statement models an expression that is used as a statement. Usually, this is a function call or an assignment. For example, in the Csharp language, a call to write something to the console is done though the `Console.WriteLine()` function call. In the _Code Model_, this would be modelled as an _Expression Statement_ containing a _Function Call_ expression.

=== Let

Many of the expressions are straightforward and are directly copied from popular object-oriented and procedural programming languages. However, transforming object-oriented code can have its hassles. If we want to transform the body of a method with procedural statements, it can become quite tricky when working with both expressions and statements. For instance, let's say we want to transform the following snippet of code:

```cs
...
double average = [1, 2, 3, 4, 5].Sum() / 5d;
...
```

to

```cs
...
int[] array = [1,2,3,4,5];
double average = array.Sum() / 5d;
...
```

Doing this conceptually may not seem hard: we just extract the array to a new variable and replace the array with the new variable. However, doing this with a generic algorithm requires the algorithm to keep track of where we are in the method body as we need to insert a statement in the list, and requires a separate traversal of the expression to replace the list. This example signifies that this seemingly simple refactor step requires a complex algorithm.

As such, the _Code Model_ introduces a known concept in functional programming languages: the _Let_ expression. This expression essentially models the assignment statement, but does this in an expression. As such, the statements from above can be expressed in one expression:

```hs
let average = (
  let array = [1,2,3,4,5]
   in array.Sum() / 5d;
) 
in ...
```

This simplifies the algorithm described above, as we only need to concern with expressions. Instead of keeping track of where we are, we only need a single traversal of the expression. We insert a _Let_ statement around the expression and replace the list with the variable.

=== Types

_Types_ are an important notion in the _Code Model_. Many high-level programming languages are strictly typed languages, which means that everything should have a type at compile time. As such, our model requires the typing of every single node in the syntax tree.

Many types are automatically converted when we convert the compute graph to the code layout model. Furthermore, many expressions and statements do not require their own types, but can infer their types from the types of their children. For instance, take the `ListExpression` which is always of the `ListOf` complex type. The `ListExpression` models a sequence of values of the same type $tau$, and as such, the `ListExpression` will always be of type `ListOf(`$tau$`)`

==== Precision
As we discussed in @sec:structural-phase, Excel does not differentiate between integers and doubles. Under the hood, Excel follows the IEEE 754 Floating-Point Arithmetic specification @microsoft_floating-point_nodate. It utilises a double precision floating point value for the implementation of 'numeric values' @microsoft_floating-point_nodate. This results in 15 digits of significant precision @noauthor_ieee_2019. Within this Excel Compiler, we also use double precision floating point values for representing the numeric values.

Precision is key, especially in the actuarial calculations context. We do not want to lose a few decimal due to precision issues, which would propagate and mean that the pension fund would be paying more or---even worse---less than what you should have gotten.

== Creating the model

Having covered the most important elements of the _Code Model_, we can dicuss how to create it from the _Compute Model_. This is done in four steps. First (1), we transform the _Compute Model_ into the _Code Model_ by mapping the different nodes from the _Compute Model_ to _Expressions_ in the _Code Model_. Simultaneously, we introduce variables by using the references to the _Structure Model_ found in the _Compute Model_. Then we iterate upon that model and (2) inline variables and (3) transform the _Let_ expressions into statements. Finally (4), we emit the code layout model as code to the disk. In the upcoming sections, we discuss these steps further.

=== Variables
The first step of the code layout model is the introduction of variables. LIke we covered in @sec:compute-phase, the _Compute Model_ is essentially one big expression that needs to be evaluated. However, emitting this big expression will significantly decrease the readability of the program. As such, we need to introduce variables to make sense of the big sub-expressions.

Every _Compute Unit_ contains the location it originated from in the _Spreadsheet_ of the _Structure Model_. In order to increase legibility and resemblance to the original Excel file, we use these locations to store computations. 

In more precise terms, we use a top-down approach, starting at a root of the _Compute Graph_, and transform every node to a _Code Model_ node. Every time we encounter a _Compute Unit_ that is from a new cell, we begin constructing a new variable and put the other---already constructed one---in a _Let_ expression. In other words, we recursively build a big expression of _Lets_.

=== Inlining
A common practice in an Excel file is to display the values from another sheet in a separate cell, and then use that cell for calculations in the sheet. This essentially makes the display cell a proxy to the other sheets. As a result of the variable creation compiler step discussed in the previous section, this creates a variable that is being assigned to another variable:

```cs 
double interestJ12 = interestF65 - interestF5 - interestJ11;
double monthlyBudgetReportJ7 = interestJ12;
double monthlyBudgetReportD10 = monthlyBudgetReportJ7;
```

In this optimisation pass, these proxy variables will be removed. We do this by refactoring the `Let` nodes, checking if a `Let` node contains another `Let` node with the assignment just a variable name. In the end, we will remove this node, and just assign the value directly to the last variable. As such, the example above becomes:

```cs
double monthlyBudgetReportD10 = interestF65 - interestF5 - interestJ11;
```

and is instantly more readable.

=== Statements
After inlining, we begin the language-specific part, as we need to convert the _Let_ expression to statements, since Csharp does not support the _Let_ expression. As such, we employ a straightforward algorithm that converts every assignment in every _Let_ expression to a statement. The following code layout model would be transformed:

```
DeclarationStatement z (Let y = 10
                        in let x = 10
                           in x + y)
```

into:

```
DeclarationStatement y 10
DeclarationStatement x 10
DeclarationStatement z (x + y)
```

The current layout model is looking more and more like a high level programming language. This example also exemplifies the versatility of the code layout model, as we can have two different styles for variable declaration that are highly coupled in parallel. This versatility is crucial when supporting different programming languages, and makes the Excelerate compiler ready for future expansion.

=== Emission <subsec:code-model:emission>

Just before emission, at the end of the _Code Phase_, we have constructed a language-agnostic representation of the Excel computation. The final challenge is to convert this abstract model into concrete compilable code. In this thesis, we chose C\# for the target or destination language. In this subsection, we briefly cover the Roslyn API to explain what it does. Then we dive into the code generation and explain how we map the code: a straightforward process. Finally, we discuss some of the subtleties needed to make the project actually compile.

==== Roslyn API
The Roslyn API is an open-source .NET compiler platform developed by Microsoft. It exposes the whole compiler process, from internal data structures to transformations, to the programmer and let's the programmer use the compiler within the C\# language itself. 

Like we spoke about earlier, the Roslyn API is flexible and expressive. However, this flexibility comes at a price since it demands explicit syntax and is able to produce invalid C\# code when misconfigured. While there are helper APIs to combat this issue, they are still very primitive, verbose, and allow for uncompilable code. Hence, we rely on the stricter semantics the _Code Model_ grants.

It is important to emphasize that Roslyn, by design, does not improve the code generated by the layout model. We only define what it has to output and it renders this to a source file. This places the burden of correctness on the layout model and the transformations leading up to emission. This design choice ensures transparency and reusability: every optimisation, transformation, or refactoring is explicit in the layout or preceding passes, and not hidden inside the emission step for a specific language. This step does, however, apply some language-specific transformations to improve readability. These optimisations include choosing for one-liner if-statements instead of full bodied if-statements or making sure the latest language features are being used. It does this on the Roslyn syntax tree as it is being generated.

==== Mapping
Translating from the _Code Model_ to the Roslyn syntax tree is fairly straightforward. Statements and expressions are recursively mapped to Roslyn’s corresponding syntax nodes. For many constructs, this direct, and not much work is needed.  For example, a list initialisation in the layout model becomes an object creation with a collection initializer in C\#. For others, such as properties and functions, we need to see if optimisations can be made to the code layout, such as using onliners with expression bodies versus full scoped bodies.

Furthermore, we rely on a few helper methods from the _Code Model_ to create constructors for types, as they are not explicitly defined in the model but rather derived from the settable data in the structures.

==== Layout
During emission, we must also address the organisation of code into files, namespaces, and classes. The code layout model describes a project as a collection of classes and methods, but leaves file system organization abstract. The emission pass generates a dedicated C\# file for each top-level class, placing them within a common namespace so they can be accessed easily. This is a common practice in Csharp @microsoft_net_2025. 

= Reflecting on the Compiler

The three phases of the compiler: _Structure_, _Compute_, and _Code_ produce a class library with the same semantics as the Excel file. In this section, we discuss the readability of the code, reflecting on the current compiler, and highlighting areas that can be improved.

== Two Examples

Before we begin the discussion, we present two examples of Excel files that have been compiled to Csharp. 
These files are small so the code fits on the page. The first spreadsheet is the same as @sps:hlo:spreadsheet1 in the High Level Overview in @sec:hlo. It describes a calculation of the Difference between Projected and Actual income. This example has many independent code paths, such as the calculation of the differences: to calculate the difference of one row, we do not have dependencies on the other row.

#figure(
  spreadsheet(
    columns: 4,
    [], [*Projected*], [*Actual*], [*Difference*],
    [Income 1], [6.000], [5.800], [`=C2-B2`],
    [Income 2], [1.000], [2.300], [`=C3-B3`],
    [Extra Income], [2.500], [1.500], [`=C4-B4`],
    [TOTAL], [], [], [`=SUM(D2:D4)`]
  ),
  caption: [A extract of the 'Family monthly budget' sheet in the 'Family monthly budget'. This spreadsheet calculates the differences in projected and actual income.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:structure-reflection:spreadsheet1>

@code:structure-reflection:spreadsheet1 shows the compiled code if we run the compiler with `D5` specified as output cell. The _Inline Variables_ compiler step has inlined the constants into the difference calculations.

#figure(
  ```cs
  public class Program
  {
      public double Main()
      {
          double sheet1D2 = 5800 - 6000;
          double sheet1D3 = 2300 - 1000;
          double sheet1D4 = 1500 - 2500;
          double sheet1D5 = new List<double> { sheet1D2, sheet1D3, sheet1D4 }.Sum();
          return sheet1D5;
      }
  }
  ```,
  caption: [A compiled version of the spreadsheet in @sps:structure-reflection:spreadsheet1 using the basic compiler described in @chapter-compiling-excel.],
  placement: auto,
)<code:structure-reflection:spreadsheet1>

The second spreadsheet contains a linear calculation path, calculating the interest and current balance in a savings account. It can be seen in @sps:structure-reflection:spreadsheet2. This spreadsheet was compiled using the 'basic' compiler with `C5` as output cell configured. The code can be found in @code:structure-reflection:spreadsheet2. 

#figure(
  spreadsheet(
    columns: 4,
    [], [*Interest*], [*Balance*], [*Deposit*],
    [May], [], [10 000], [],
    [June], [=$0.01 dot #[C2]$], [=C2+B3+D3], [500],
    [July], [=$0.01 dot #[C3]$], [=C3+B4+D4], [500],
    [August], [=$0.01 dot #[C4]$], [=C4+B5+D5], [500],
  ),
  caption: [An extract of the 'Interest' sheet in the 'Family monthly budget' Excel file. This spreadsheet calculates the ],
  placement: auto,
  supplement: "Spreadsheet",
)<sps:structure-reflection:spreadsheet2>

#figure(
  ```cs
  public class Program
  {
      public double Main()
      {
          double sheet1C2 = 10000;
          double sheet1B3 = 0.01 * sheet1C2;
          double sheet1C3 = sheet1C2 + sheet1B3 + 500; 
          double sheet1B4 = 0.01 * sheet1C3;
          double sheet1C4 = sheet1C3 + sheet1B4 + 500;
          double sheet1B5 = 0.01 * sheet1C4;
          double sheet1C5 = sheet1C4 + sheet1B5 + 500;
          return sheet1C5;
      }
  }
  ```,
  caption: [A compiled version of the spreadsheet in @sps:structure-reflection:spreadsheet2 using the basic compiler described in @chapter-compiling-excel.],
  placement: auto,
)<code:structure-reflection:spreadsheet2>

#place.flush()

== Missing structure

When evaluating the compiled code, two things are investigated: the semantics and the readability and extensibility. The compiler should emit code that is semantically equivalent to Excel---without this, the compiled code would require extra human intervention to be useful.
Running the evaluation as described in @sec:eval:methods, it produces the same output as Excel. As such, we can assume the semantics of Excel are preserved.

As discussed previously in @sec:intro:idiomatic-code, the code the compiler emits should be readable and extensible: the code written should be on the same level as how a good software engineer would have written the code. Applying this arguably subjective definition on the two samples, we argue a few problems emerge.

@code:structure-reflection:spreadsheet1 is considered good, simple code without duplication and contains use of idiomatic LINQ constructs (SUM) rather than using loops. This reduces the cognitive complexity and increases comprehensibility and readability. On the other hand, the code in @code:structure-reflection:spreadsheet2 is less readable. It contains lots of duplicate code, since the calculation for the balance is repeated three times. Furthermore, since the values in the _Deposit_ column are inlined, it is not clear that these are variable values and look like one single constant value added to the balance.

The compiled versions are currently without parameters. When we add parameters to @code:structure-reflection:spreadsheet1, we quickly discover a problem: there will be too many parameters. For every row, we would have two parameters, resulting in six parameters for this simple problem. Expanding the spreadsheet would result in even more parameters. This could be fixed with an _Input_ class containing all inputs as properties, but that is not idiomatic.

Another clear issue in both listings is the lack of extensibility. The Excel spreadsheets may be expanded to include the month _September_ in the savings calculation, or remove a source of income in the _family budget_ In both cases, the compiled code would have to be recompiled. Ideally, the compiled code is flexible that it should not have to be recompiled, since the underlying computational model does not change: the only change is the extra argument in the SUM and how that argument is calculated, but that is done in the same way as the other rows.

== Guided by structure

#figure(
  ```cs
  public record Income(double Projected, double Actual) {
    public double Difference => Actual - Projected;
  }
  
  public class Program
  {
      public double Main()
      {
          List<Income> incomes = [
            new(6000, 5800),
            new(1000, 2300),
            new(2500, 1500)
          ]
          
          double sheet1D5 = incomes.Select(i => i.Difference).Sum();
          return sheet1D5;
      }
  }
  ```,
  caption: [A compiled version of the spreadsheet in @sps:structure-reflection:spreadsheet1 using the basic compiler described in @chapter-compiling-excel.],
  placement: auto,
)<code:structure-reflection:spreadsheet1-solution>

A way to solve the problems is to analyse the code and introduce refactoring. For instance, the repetition in @code:structure-reflection:spreadsheet2 could be fixed by statically analysing the code, discovering this recurrence of calculations and abstracting the repetition. However, for the problem of extensibility and parameters, the fix is not easy. A solution would be to introduce a structure that models the data in such a way that it fits the computations, like in @code:structure-reflection:spreadsheet1-solution. This introduces a new class `Income` that models the rows found in the spreadsheet. It also creates a computed property `Difference` that would have removed duplicated code. Furthermore, it uses the `Select` LINQ statement to only sum the differences. Furthermore, if we take this new `incomes` local variable as parameter instead, adding or removing rows from the income is easier than ever without creating any more parameters---increasing the extensibility at no expense of readability.

However, creating these structures with the _Code Model_ alone will be hard: detecting which operations should be part of the structure is hard since the scope of the structure is highly dependent on the context. Besides, in small examples like @sps:structure-reflection:spreadsheet1 and @sps:structure-reflection:spreadsheet2 this might be do-able but uncovering possibly multiple structures within a large spreadsheet with many data is highly complex.

That said, a similarity can be found between the newly added `Income` structure and the _structure_ of the _Spreadsheet_ in @sps:structure-reflection:spreadsheet1. If we see this spreadsheet as a table, with columns _Projected_, _Actual_, and _Difference_, the structure basically describes one row of this table. This demonstrates the fact that the semantics is often encoded in the spreadsheet structure.  The meaning of the nodes used in the _Code Model_ can be inferred from the structure. For instance, The `SUM` in `D5` does not just denote the sum of the range, but more likely means the total of the _difference_ column of the table.

When we use this heuristic as part of the compiler, we are able to create more readable code that fits more closely to the semantics of the spreadsheet. In the next chapter, we introduce exactly this form of compilation, called _structure-aware compilation_, that takes structural information and metadata into account in order to improve the readability of the emitted code.