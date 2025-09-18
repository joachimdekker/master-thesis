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

// #show figure: it => {
//   show raw.where(block: true): r => {
//     set text(font: "JetBrains Mono", size: 0.7em)
  
//     // The raw block should be encased in a box
//     align(left,
//       block(r, fill: luma(97%), inset: 1em, radius: .25em, width: 100%)
//     )
//   }

//   show raw.where(block: false): r => {
//     set text(font: "JetBrains Mono", size: 1em)
//     r
//   }
  
//   it
// }


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
  kind: "spreadsheet",
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

The next subsections describe the structures we encountered in the repository. These definitions are loose, and we will introduce the heuristics for detecting them in @subsec:structural-model:heuristics. In other words, what the compiler considers to be a _Chain_ or _Table_ is covered in the next section. What we describe in this section is the background of these structures, what separates them from each other and what they mean semantically.

=== Tables<subsec:def:table>

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
    [_TOTAL_], [], [=SUM(C3:C6)], [=SUM(D3:D6)], [=SUM(E3:E6)],
  ),
  caption: [An example of a table structure found in a _Spreadsheet_. Cell `A1` denotes the title, range `A2:E2` describes the headers of the table. The actual data can be found in between the headers and the column in `A3:E6`. Finally, the footer is described in `A7:E7`.],
  kind: "spreadsheet",
  supplement: "Spreadsheet",
)<sps:structures:table>

Tables are essential in Excel. Almost all _Spreadsheets_ contain some sort of Table. This is not surprising given the inherent tabular presentation of the Excel grid. Furthermore, _Tables_ are an explicit feature of Excel, as we saw in @sec:intro:excel.

Semantically, a table represents a list of objects. These objects are described by the data in the rows. For instance, in the table in @sps:structures:table, the table describes a list of expenses. The properties of the objects are determined by the column names. In the aforementioned table, this means that the properties of an expense are a description, a category, the projected cost, the actual cost and the difference.

The table pattern that was often observed was a rectangular region in the _Spreadsheet_ containing data, a header and sometimes a title or a footer. The columns of the table may contain data, or they contain a formula. If a column has the same formula in every row, that column is a _Computed Column_.

The data in the table does not have to be constant, it may be expressed through a formula reference expression. For instance, instead of having a constant `40.00` for the projected cost for the Extracurricular activities, we could instead have a reference to another cell in the spreadsheet `X1` that would calculate this cost (but is not shown here).

#figure(
  image("sps-structures-table-dependencies.png"),
  caption: [The dependencies for the rows in @sps:structures:table. Cells only depend on cells in their rows.],
  kind: "spreadsheet",
  supplement: "Spreadsheet",
)<sps:structures:table-dependencies>

An important property of the _Table_ is that there are no dependencies between columns. Columns in a _Table_ are independent of each other (apart from the footer, which may reference all the columns). @sps:structures:table-dependencies shows the dependencies in @sps:structures:table. All these dependencies are horizontal: the cells only depend on other cells in their respective rows. As such, @sps:structures:chain is not a table, since `C5` and `B5` contain references to `C4`, which is not allowed in a table. Instead, we have identified another structure that support this.

=== Chains<subsec:def:chain>

Chains are similar to tables, but they allow for dependent rows. Unlike _Tables_, chains are not a feature of Excel but are still common in Excel sheets since they describe a recursive calculation. Semantically, a chain represents a collection of objects that are in relation to eachother.

Take @sps:structures:chain, it represents a collection of balances for every month. Every new balance calculation is dependent on the previous balance. An object in a chain can just be constant data, such as the _Months_ or _Deposit_ column, or be calculated from the previous row.

This calculation is a recursive definition. As @sps:structures:chain-dependencies shows, the previous row is need to calculate a cell in the _Money_ or _Interest_ column. As with every recursive definition, a chain also has base cases, represented in `C2` for @sps:structures:chain. Columns that use these recursive definition are called _Recursive Columns_. 

#figure(
  spreadsheet(
    columns: 4,
    [*Months*], [*Interest*], [*Money*], [*Deposit*],
    [May], [], [10 000], [],
    [June], [=$0.01 dot #[C2]$], [=C2+B3+D3], [500],
    [July], [=$0.01 dot #[C3]$], [=C3+B4+D4], [500],
    [August], [=$0.01 dot #[C4]$], [=C4+B5+D5], [500],
  ),
  caption: [An example of a chain-table. The chain is visible in the _Interest_ (B) and _Money_ (C) column of the table, where the interest calculated is based on the previous month.],
  placement: auto,
  kind: "spreadsheet",
  supplement: "Spreadsheet",
)<sps:structures:chain>

#figure(
  scale(x: 50%, y: 50%, image("sps-structures-chain-dependencies.png"), reflow: true),
  caption: [The dependencies for the rows in @sps:structures:table. Cells only depend on cells in their rows.],
  kind: "spreadsheet",
  placement: auto,
  supplement: "Spreadsheet",
)<sps:structures:chain-dependencies>

#place.flush()


= Changes to the Compiler<sec:excelerate:compiler-changes>

#figure(
  image("image.png"),
  caption: [The new phases of the compiler.]
)

Now that we have a intuition for the structures found in the spreadsheet, we introduce Excelerate: an improved compiler which uses _structure-aware compilation_ using the structures found in the previous section to emit more readable code. Excelerate builds upon the 'basic' compiler introduced in the previous chapter. It extends the _Structural_ and _Compute Model_ to allow for these new structures. In this section, we briefly discuss what is needed for the compiler to utilise these structures.

== Changes in Input

A great side-effect of _structure-aware compilation_ is that we can use these structures as input. The data residing in the structures does not have to be used. The programmer should be able to provide their own data. With the introduction of the _Chain_ and the _Table_, we can exploit the structure to provide custom data by parametrising the structure, laying the burden of creating the actual structure on the developer working with the final code. This enables flexibility and could improve readability as we saw in @sec:basic-compiler:reflection.

== Detecting the structures

One of the immediate problems that arises is the detection in the spreadsheet. This detection should be done during the _Structural Phase_ as it relates to the structure of the spreadsheet.

Ultimately, this problem can be decomposed into two subproblems:
1. Finding regions in the spreadsheet that may contain a structure.
2. Classifying these regions.

A closer look at the structures we found reveals a common property: the chain and the tables are both rectangular structures and are often densely populated with data or formulae. As such, potential regions should exploit this property. In @subsec:structural-model:areas we introduce _Areas_, which exactly exhibit this property. 

Then, once we have found potential regions, we should try to classify them. In @subsec:structural-model:heuristics, we discuss heuristics for both structures. As we will see, these heuristics are able to capture most structures, but there are some caveats. The found structures will be stored in the _Structural Model_.

== Linking the structures<subsec:changes:compute>

The _Structural Model_ merely records that a structure exists, it intentionally only captures the spreadsheet and does not modify it. For instance, detecting @sps:structures:table does not change any references made to the cells in the table, they still reference the cells in the spreadsheet: `E3:E6` is still a range but actually represents the data in the `Difference` column of the table. As such, while structures are found within the spreadsheets, they are separate from the spreadsheet.

To express this, we should inject the structure into the _Compute Model_ so that `E3:E6` does not represent a range but instead a column of the table. Doing this in the _Compute Graph_ is not efficient. For every structure, we would need to traverse the whole graph to update the references made to the table. 

Instead, we introduce the _Compute Grid_, a part of the _Compute Model_ that mixes the grid-like structure of the _Structural Model_ but utilises _Compute Units_ to represent the values and computations. 
Using the _Compute Grid_, we can inject special _Compute Units_ describing the structures. For instance, the footer of the table will be updated to a _Table Footer Compute Unit_ that describes the operation done on the column of the _Table_. When we transform the _Compute Grid_ into the _Compute Graph_, we make sure that references like `E3:E6` actually reference the structure.

== Structure code
The references to the structures should also be reflected in the code. The best way to do this is to represent every structure as their own class. We need to add steps to the _Code Phase_ to generate these structures and add constructors in the main function to create the structures to utilise them.

---

These changes should incorporate the structures into the compile flow 
In the rest of this chapter, we describe what changed in the different models, and how we integrate the structures into the compiler to make more readable code.

= Finding the Structures

The first step in integrating the structures is finding them. We already discussed a way to do this: we first find regions in the spreadsheet that may contain a structure and then try to classify these candidates. To facilitate this detection mechanism, we need to change the _Structural Model_ to allow for _Structures_ to be stored. In this section, we discuss these new additions to the _Structural Phase_.

== Changes in the model

#figure(
  ```cs
    -- record Workbook(string Name, Table[] Tables, Spreadsheet[] Spreadsheets);
    ++ record Workbook(string Name, UserTable[] UserTables, Structures[] Structures, Spreadsheet[] Spreadsheets);
    
       record Spreadsheet(string Name, Set<Cell> Cells);
    
       record Cell(Location Location);
         -> record EmtpyCell();
         -> record ValueCell<T>(T Value);
         -> record FormulaCell(Formula Formula);
      
    ++ record Structure(string Id, Range Location)
    ++ -> record Table(Column[] columns)
    ++ -> record Chain(Column[] columns)
    ++
    ++ record Column(Range Location, string Name, string Type)

    -- record Table(string Name, Range Location, string[] Columns);
    ++ record UserTable(string Name, Range Location, string[] Columns);
    ```,
    caption: [Changes in the formal _Structural Model_. ]
)

The most notable change made in the model is the introduction of the _Structure_. This describes a structure in the spreadsheet. Every structure has a unique identifier and a location denoted by a _Range_. The identifier is derived from the title of the structure, or the location. For example, the identifier of @sps:structures:table would be _Expenses_ since it has a title. On the other hand, @sps:structures:chain does not have a title, and would get the `Sheet1A1D5` identifier.

As we have discussed, a _Structure_ can be either a _Table_ or a _Chain_. Both _Tables_ and _Chains_ have columns. These columns always have a location and a name. Based on the content, they are assigned a type. The possible types are _Data_, _Computed_, and _Recursive_. In @subsec:structural-model:heuristics, we cover what these types mean and when they are assigned.

The _Table_ type was changed to _User Table_ to distinguish it from the new _Table_ structure. This used to represent an Excel Table (which is marked by the user) and is used in _Table References_. However, with the introduction of the more generic _Structures_, this node has become near obsolete. In Excelerate, this is purely used for the discovery of _Structures_: we try to convert the cells in the range of the _User Table_ to a _Table_ structure.

== Areas<subsec:structural-model:areas>

Before we can classify if a region is a table or a chain, we need to find this region first. This region in a _Spreadsheet_ that could contain a _Structure_ is called an _Area_ within Excelerate.

=== Detecting Areas
Detecting these areas correctly is hard. Excel does not explicitly store these areas. What we perceive as a nicely bounded table, might be hard to detect due to ambiguity surrounding gaps or decorative formatting. Gaps can mean a blank separator between areas, or just a row or column with missing data. Furthermore, formatting can mean a separation between adjacent structures, or they signal a separation between header and data. These equivocal concepts mean that it would be impossible to correctly detect all areas. That said, we can use a heuristic to infer these areas.

#figure(
  spreadsheet(
    columns: 8,
     [*Expenses*], [], [], [], [], [], [*Table 2*], [],
     [], [], [], [], [], [], [*Col1*], [*Col2*],
     [*Description*], [*Category*], [*Projected*], [*Actual*], [*Difference*], [], [890], [130],
     [Extra... activities], [Children], [40.00], [40.00], [=D2-C2], [], [187], [092],
     [], [], [], [], [=D3-C3], [], [*Table 3*], [],
     [School tuition], [Children], [100.00], [100.00], [=D4-C4], [], [*Col X*], [*Col Y*],
     [], [], [], [], [=D5-C5], [], [42], [65],
     [Concerts], [], [50.00], [40.00], [=D6-C6], [], [28], [48],
     [], [], [], [], [], [], [05], [58],
     [Movies], [Entertainment], [50.00], [28.00], [=D8-C8], [], [], [],
     [Music ], [Entertainment], [50.00], [30.00], [=D9-C9], [], [], [],
     [], [], [], [], [], [], [], [],
  ),
  caption: [An example of a spreadsheet with gaps. Four areas will be detected using the adjacency heuristic: `A1:A1`, `A3:E8`, `G1:H9` and `C11:G12`. The gap in row 10 results in the split of the table. Table 2 and Table 3 don't have an empty separation row, resulting in one area instead of two.  This highlight the difficulties of detecting areas.],
  kind: "spreadsheet",
  supplement: "Spreadsheet",
  placement: auto
)<sps:areas:limitations>

The heuristic we use in Excelerate is adjacency, where we check for continuous regions. A cell is adjacent to another cell if the edges touch each other. This means that cells that 'touch' each other on the diagonal are not adjacent. Empty cells cannot be adjacent to any other cell. Using this adjecency heuristic, we are essentially dividing the _Spreadsheet_ into groups of connected cells.

We construct an adjacency graph and find areas using the Hopcroft-Tarjan DFS algorithm @hopcroft_efficient_1973. More precisely, we use the following algorithm:

#place.flush()

1. Convert the spreadsheet to a graph. Convert every cell in a _Spreadsheet_ to a node in the graph, then for every node find the neighbours in the list by comparing their location.
2. Find the connected components using the Hopcroft-Tarjan DFS algorithm @hopcroft_efficient_1973. It outputs a set of connected nodes.
3. Create the areas. For every connected component, we construct a bounding box of the nodes in the connected component, which represents the Area.

When the areas are found, they are passed along to the next step of structure detection: the actual classification of the areas.

=== Limitations of the heuristic

@sps:areas:limitations illustrates the power and limitations of the adjacency heuristic. The adjacency heuristic detects most structures that have empty rows, like the area `A3:E8`. This is due to computed columns that require formulas in all cells of the column, also when the other columns are empty. As such, the two regions `A3:E4` and `A6:E6` are connected using this formula in `E5`---essentially acting as a bridge.

However, when this is not present, such as the missing formula in `E9`, the two regions remain split. Furthermore, another failure happens when two tables are adjacent, such as the two tables in `G1:H9`.

To address these issues, other area-detection algorithms can be applied, or the adjacency graph can be adapted to  include decorative formatting heuristics. For instance, if the background is the same color, it can also be regarded as the same area. However, this would increase the false positives when the whole spreadsheet is a single colour.

== Classifying Structures<subsec:structural-model:heuristics>

Having identified the areas in the _Spreadsheet_, we can start classifying based on the content in the structures. Classification is a two-step process. First we determine if the structure meets the requirements based on several heuristics. If it complies, the literal structure is extracted to a _Structure_ node in the _Compute Model_. In this section, we discuss the heuristics required for classification of the structures, also covering their limitations.

=== Heuristics

Before we discuss what the compiler sees as a _Table_ or _Chain_, we first stress the fact that we work with heuristics. Just like with detecting areas, correctly identifying structures is hard. Detecting all edge cases is hard or even impossible to do. For instance, when a table only has strings in the first row, there is an ambiguity between the row being a header, or just a row with string data. 

The heuristics we present here are not perfect and have their limitations. Furthermore, we require the structures to be bigger than a few rows to reduce the possibilities of false classifications. For _Tables_ and _Rows_, this means that the actual data in the table (the rows between header and footer) has to span more than one row.

=== Tables

#set enum(indent: 1em, spacing: 1em)

The characteristics of the table defined in @subsec:def:table can be used to detect the structure. To reiterate, a table is a rectangular region with optional title, headers and footer. The rows of the table do not depend on each other.

We use the following conditions to identify _Tables_ in a _Spreadsheet_. This rule-set is used as a heuristic instead of a formal specification, resulting in occasional error in classification. The key words "MUST", "SHOULD",  "MAY", and "OPTIONAL" in this document are to be interpreted as described in RFC 2119 @bradner_key_1997. The tables are detected according to the following rule-set:

+ A table MAY have a title. If the table has a title, it MUST be in the left-most cell of the first row and the rest of the first row MUST be empty.
+ A table MAY have a header. If the table has a title, the header MUST be in the second row. If the table does not have a title, the header MUST be in the first row. The header row MUST only contain string value cells.
+ A table MUST have more than one data row.
+ A data table column MUST have cells of the same type excluding empty cells.
+ A computed table column MUST only use references from the same row. All cells in the computed column MUST be the same formula, excluding the differences for row references. It should have the same 'shape'.
+ A table MAY have a footer. If the table has a footer, it MUST be the last row of the table. The operations in the cells MUST be aggregation operations. If the first column is a data column, the first cell of the footer MAY be a string value cell (for the "TOTAL" text or similar).

If the area is determined a _Table_ according to the rule-set, it is converted to an _Table_ node in the _Structure Model_. 

#figure(
  spreadsheet(
    columns: 5,
    [], [], [], [], [],
    [*Montly Expenses*], [], [], [], [],
    [], [], [], [], [],
    [*Description*], [*Category*], [*Projected*], [*Actual*], [*Difference*],
    [Extra... activities], [Children], [40.00], [40.00], [=D2-C2],
    [Medical], [Children], [], [], [=D3-C3],
    [School supplies], [Children], [], [], [=D4-C4],
    [School tuition], [Children], [100.00], [100.00], [=D5-C5],
    [Concerts], [Entertainment], [50.00], [40.00], [=D6-C6],
    [Live theater], [Entertainment], [200.00], [150.00], [=D7-C7],
    [Movies], [Entertainment], [50.00], [28.00], [=D8-C8],
    [Music ], [Entertainment], [50.00], [30.00], [=D9-C9],
    [], [], [], [], [],
  ),
  caption: [An excerpt of the Montly expenses spreadsheet of the family budget workbook.],
  supplement: "Spreadsheet",
  kind: "spreadsheet",
  placement: auto
)<sps:monthly-expenses>

The area in @sps:monthly-expenses is a table. The table has a header, which is in the first row (since the table does not have a title) and all of them are strings. When we have detected a title, headers and footers, it checks the columns of the rows in-between the header and footer to determine the type of the columns. In the case of @sps:monthly-expenses, there are four data columns and one computed-column.

We need to compare formulas to determine if a column is a computed column. To do this, we introduce the concept of the _shape_ of a formula. The _shape_ describes the relative positions of the dependencies of the cell. With the shape of a cell, we can check if a cell has the same relative dependencies as other cells in the same column. For instance, the computed column in @table:shape:example1 has two cells with different shapes, since the relative dependencies are different: The `C4` cell misses the extra `+ A3` at the end.

#figure(
  spreadsheet(
    columns: 3,
    [Data Column 1], [Data Column 2], ['Computed' Column],
    [_a_], [_x_], [`= (4 * B2) + A2`],  
    [_b_], [_y_], [`= (4 * B3)`]),
  caption: [An example of a computed column with mismatching shapes.],
  supplement: "Spreadsheet",
  kind: "spreadsheet",
  placement: auto
)<table:shape:example1>

#place.flush()

==== Limitations

One of the limitations of these rules is already discussed above: when the header row contains strings, it is unclear if this is data or the names of the headers. In this case, the compiler will interpret them as headers.

Another big limitation is the use of 'external' references: references outside the table. When this reference is constant, the shape of the formula between rows does not change, and we get the desired result. However, when a calculation references the rows in another table such as in @sps:table:limitations, the shape is different for every cell, and as such, the row is not recognised as a formula row.

#figure(
  spreadsheet(
    columns: 7,
    [], [], [], [], [], [], [],
    [*Table 1*], [], [], [], [], [], [*Table 2*],
    [], [], [], [], [], [], [],
    [*Projected*], [*Actual*], [*Difference*], [], [], [], [*Potential*],
    [40.00], [40.00], [`=B5-A5`], [], [], [], [`=C5*1.005`],
    [], [], [`=B6-A6`], [], [], [], [`=C6*1.005`],
    [], [], [`=B7-A7`], [], [], [], [`=C7*1.005`],
    [100.00], [100.00], [`=B8-A8`], [], [], [], [`=C8*1.005`],
    [50.00], [40.00], [`=B9-A9`], [], [], [], [`=C9*1.005`],
    [], [], [], [], [], [], [],
  ),
  caption: [An excerpt of the Montly expenses spreadsheet of the family budget workbook.],
  supplement: "Spreadsheet",
  kind: "spreadsheet",
  placement: auto
)<sps:table:limitations>

=== Chain



The chain as described in @subsec:def:chain can also be detected according to its characteristics. The chain table looks like a normal table but has the changed restriction that it allows for recursive columns, where a cell may depend on a cell in another row in the chain.

We use the following conditions to identify _Chains_ in a _Spreadsheet_. This rule-set is used as a heuristic instead of a formal specification, resulting in occasional error in classification. The key words "MUST", "SHOULD",  "MAY", and "OPTIONAL" in this document are to be interpreted as described in RFC 2119 @bradner_key_1997. The tables are detected according to the following rule-set:

+ A chain MAY have a title. If the chain has a title, it MUST be in a single cell in the first row and the rest of the row MUST be empty.
+ A chain MAY have a header. If the chain has a title, the header MUST be in the second row. If the chain does not have a title, the header MUST be in the first row. The header row MUST only contain string value cells.
+ A chain MUST have more than one data row.
+ A chain MUST have one or more initialisation rows.
+ A data chain column MUST have cells of the same type excluding empty cells.
+ A computed chain column MUST only use references from the same row. All cells in the computed column MUST be the same formula, excluding the differences for row references. It should have the same 'shape'. All cells in the computed column MUST not reference a cell from a calculated chain column.
+ A recursive chain column MUST have all cells use references from the same row or the rows above it except for the base cases. The base cases MUST be in a higher row than recursive formula cells.. All recursive formula cells in the chain column must have the same shape.
+ A chain MUST have at least one recursive chain column.

Just like with a _Table_, we first check for titles, headers and footers, since they affect the range of the data. Then, we determine the type of columns of the _Chain_ based on the contents of the column. Again, we use the _shape_ of a cell to check if it is _Computed_ or _Recursive_.

When a computed chain references a recursive chain column, it will automatically become an recursive chain column since we need the value of that cell, which can only be calculated recursively. Furthermore, a big difference between the table and the chain is the initialisation. We detect the initialisation by looking at the cell shape. When a column needs an initialisation, it will need a different shape for the base case and formula cells to calculate the rest.

#figure(
  spreadsheet(
    columns: 6,
    [*Date*], [*_Interest_*], [*Deposit*], [*_Total_*], [], [_Interest Rate_],
    [01/03/2021], [], [], [10000], [], [0.015],
    [01/04/2021], [`= D2 * F2`], [500], [`= D2 + B3 + C3`], [], [],
    [01/05/2021], [`= D3 * F2`], [500], [`= D3 + B4 + C4`], [], [],
    [01/06/2021], [`= D4 * F2`], [500], [`= D4 + B5 + C5`], [], [],
    [01/07/2021], [`= D5 * F2`], [500], [`= D5 + B6 + C6`], [], [],
    [01/08/2021], [`= D6 * F2`], [500], [`= D6 + B7 + C7`], [], [],
    [01/09/2021], [`= D7 * F2`], [500], [`= D7 + B8 + C8`], [], [],
  
  ),
  caption: [An example of a chain that is correctly detected. The chain contains an initialisation row in row 2. ],
  supplement: "Spreadsheet",
  kind: "spreadsheet",
  placement: auto
)<chain:example1>

A great example of the conversion of a chain is the savings spreadsheet, for which an excerpt can be found in @chain:example1. Note that we find two areas: A1:D8 and F1:F2. Only A1:D8 is detected as a chain. We can see two recursive columns: _Interest_ and _Total_ which both reference the previous columns.

= Embedding the Structures

Having extracted the structures out of the _Structural Model_, we need to incorporate them into the _Compute Model_. As we discussed in @subsec:changes:compute the _Structural Model_ does not change the formulae to account for the newly found structures. It is up to the _Compute Model_ to update the _Compute Units_. 

All references that reference the structures need to be updated to refer to the new structures. For instance, a range may be referring to a column of a table. Instead of a range, we would like to refer to this as a _Column Reference_ that also references the table.

Doing this replacement in the graph can be computationally expensive. We would have to traverse the graph for every structure to make sure the dependencies are correctly linked to the structures. However, due to the grid-like nature of the structures, it is easier insert them into the _Compute Model_ before we turn it into a graph. Hence, in this section, we introduce a new form of the _Compute Model_ called the _Compute Grid_.

To accommodate for the structures, the _Compute Model_ had to change slightly. We first introduce the _Compute Model_ and the required additions to the _Compute Model_ in the form of new _Compute Units_. Then, we discuss how to populate the new compute grid and other changes made to the compilation pipeline. 

== Compute Grid


We introduce the _Compute Grid_: A sparse two-dimensional grid-like data structure that models the computation done in every individual cell. Essentially, the Compute Grid is the stepping stone between the structure model and the compute graph: it retains the grid-like structure of the structure model, while modelling the calculations according to the compute model. It allows for more granular compilation to the compute model, since we can first convert the cells to the compute model, then embed the structures into the _Compute Model_, and link the _Compute Units_ afterwards.

#figure([
  #set text(0.7em)
  #grid(
    columns: 2,
    gutter: 20pt,
    spreadsheet(
      columns: 5,
      [], [], [], [], [],
      [*Expenses*], [], [], [], [],
      [], [], [], [], [],
      [*Description*], [*Category*], [*Projected*], [*Actual*], [*Difference*],
      [Activities], [Children], [40.00], [40.00], [=D2-C2],
      [Medical], [Children], [], [], [=D3-C3],
      [School supplies], [Children], [], [], [=D4-C4],
      [School tuition], [Children], [100.00], [100.00], [=D5-C5],
      [_TOTAL_], [], [], [], [=SUM(E5:E8)],
    ),
    spreadsheet(
      columns: 5,
      [], [], [], [], [],
      [], [], [], [], [],
      [], [], [], [], [],
      [], [], [], [], [],
      [], [], [`40.00`], [`40.00`], [`F('-', [D5, C5])`],
      [], [], [`0`], [`0`], [`F('-', [D6, C6])`],
      [], [], [`0`], [`0`], [`F('-', [D7, C7])`],
      [], [], [`100.00`], [`100.00`], [`F('-', [D8, C8])`],
      [], [], [], [], [`F('SUM', [E5,E6,E7,E8])`],
    ),
  ),
  ],
  caption: [An example of the conversion of the _Structural Model_ to the _Compute Grid_ with `E9` taken as output. Many redundant cells are removed. The `F` is a shorthand for a _Function Compute Unit_.],
  supplement: "Spreadsheet",
  kind: "spreadsheet",
  placement: auto
)<sps:compute-grid:example>

@sps:compute-grid:example shows an example conversion from the _Structural Model_ to the _Compute Grid_. `E9` is taken as output. During the conversion to the _Compute Grid_, we use the input to the compiler to adjust which cells are converted to the _Compute Grid_. As a result, the _Compute Grid_ removes all cells that are not used by the calculations. 

In order to support the _Compute Grid_, we need a way to express references to cells, ranges and structures in the _Compute Model_. As such, we need to update the model to allow for these nodes.

== Compute Model

#figure(
  ```cs
       record ComputeUnit(Type type, Location Location, ComputeUnit[] Dependencies);
       -> record Nil();
       -> record ConstantValue(object Value);
       -> record Input(string Name);
       -> record Function(string Name);
    ++ -> record GridReference()
    ++    -> record CellReference(Location location)
    ++    -> record RangeReference(Range range)
    ++    -> record StructureColumnReference(string structureId, string columnName)
    ++    -> record StructureCellReference(string structureId, string columnName, int index)
    ++    -> record StructureFooterReference(string structureId, string columnName)
    ++
    ++ record Structure(string Id, Range Location)
    ++ -> record Table(Column[] columns, TableData data)
    ++ -> record Chain(Column[] columns, ChainData data)
    ++
    ++ record Column(string Name, ComputeUnit? Footer)
    ++ -> record DataColumn()
    ++ -> record ComputedColumn(ComputeUnit computation)
    ++ -> record RecursiveColumn(ComputeUnit computation)
    ++
    ++ record StructureData(string structureId)
    ++ -> record TableData(Dict<string, ComputeUnit[]> columns)
    ++ -> record ChainData(Dict<string, ComputeUnit[]> dataColumns, Dict<string, ComputeUnit[]> baseCases)
    ```,
    caption: [Additions to the formal _Compute Model_. ]
)


The _Compute Model_ is unable to express explicit references to other cells that we need in the _Compute Grid_. As such, we need to add nodes to the IR that support them. These nodes resemble the _References_ in the _Formula Language_. We also add the structures and columns with references that directly point to structures.

=== References

Within the _Compute Model_, references are used to denote a link to a _Compute Unit_ in another cell of the _Compute Grid_. We distinguish between _Simple_ and _Dynamic_ references. _Simple_ references are the same as the references in the _Formula Model_, they reference a static region on the grid, like _Cell References_ and _Range References_. These nodes will be removed once we convert the _Compute Grid_ to the _Compute Model_, since they can be replace by the actual node(s). For instance, a _Cell Reference_ can be replace by the _Compute Unit_ in the cell it references.

For structures, we need references that reference a structure instead of a location on the grid. We call them _Dynamic References_. These references can be to a column, a footer or an individual cell in the structure. They always contain the name of the structure and the column they refer to. 

=== Structures

The structures in the _Structure Model_ need to be transformed to the _Compute Model_. Besides copying the name and range, this transformation primarily extracts the computations from the structures. The structures both have _Column_ nodes that describe the type of column it is. Like the _Structure Model_, we distinguish between three column types: _Data_, _Computed_, and _Recursive_. The latter two use a _Compute Unit_ to describe the repeated calculation.

==== Structure Data
The data of the columns is stored separately in a _Structure Data_ node to maintain compute and data separation. When a structure is marked as input, this data does not have to be extracted. For non-input structures, only the data of the _Data Column_ and the base cases of the _Recursive Columns_ is extracted. The data is stored as a list of _Compute Units_, since the data does not have to be constant and may contain references to other _Compute Units_.

== Changes in the Compiler Pipeline

The changes in the _Compute Model_ allow for structures to be used. However, the current compiler pipeline does not utilise the structures from the _Structural Model_. As such, the steps in the _Compute Phase_ has to be altered. 

Currently, we employ type inference and input substitution. To adjust for the new structures, we remove the transformation step between the _Structural Model_ and the _Compute Graph_. Instead, we start with a similar transformation from the _Structural Model_ to the _Compute Grid_. Then, we insert the input as described in @sec:compute-phase and extract the _Structure Data_. Afterwards, we insert the _Structures_ into the _Compute Grid_.

Then, having a fully constructed _Compute Grid_, we convert it to the _Compute Graph_ where we infer types and do one last compiler pass to ensure all references correctly reference the structures.

In the next sections, we discuss the additions and changes made to the compiler.

=== Structure Model to Compute Grid
For the conversion of the _Structure Model_ to the _Compute Model_ we look at the outputs the user has specified and convert that cell from the _Structure Model_ to a tree of _Compute Units_. For every _Value Cell_, we create a _Constant Value_. For every _Formula Cell_, the formula is transformed from the _Formula Model_ to the _Compute Model_. 

This tree of _Compute Units_ is placed inside the grid. Then, we look at the referred location (in the _Spreadsheet_) of _Reference_ nodes inside the _Compute Units_ and convert those cells as well. This has the nice side-effect that cells that are not used in the final computation are not present in the _Compute Grid_.

The conversion of the _Structural Model_ to the _Compute Grid_ is a simplified version of the algorithm described in @subsec:compute-graph:conversion that converts the _Structural Model_ to the _Compute Graph_. The only difference is that we skip the last step: we do not link the converted _Compute Unit_ to their parent.

==== Structures 
The _Structures_ in the _Structure Model_ also get converted to the _Compute Grid_. This is done _after_ the initial grid is constructed since we need it in constructing the _Structures_. For every structure in the _Structural Model_, all cells in a column are checked if they have any dependents (i.e. it is present in the _Compute Grid_). If the whole column is not present in the grid, it is pruned. The column type is always preserved.

=== Extracting Data

In the case that the user did not mark a structure as input, we need to capture the contents. This data is captured in a separate _Structure Data_ object, separate from the compute grid and the structure. We store a reference to this object in the structure. As described before, the data is only extracted from non-computed and non-recursive columns.

The data is copied from the _Compute Grid_ and stored as _Compute Units_. Since it is possible to reference data outside the structure through compute unit references. What data is stored depends on the structure. The _Table_ only stores the data for the _Data Columns_. The _Chain_ stores data for the _Data Columns_ and the base cases for the _Recursive Columns_.

It is important that this compilation step is performed _before_ we embed the structures since that step replaces the contents in the cell with references to the structures.

=== Embedding Structures

When the data is succesfully extracted and saved in the structure, we can replace the cells of the structure in the _Compute Grid_ with references to the structure. Every cell in _Compute Grid_ in the region of the _Structure_ will be replaced with a reference. The cells in the columns are replaced by a _Structure Cell Reference_, referencing the structure and the column of the cell. The footers are replaced with _Structure Footer Reference_, referencing the footer operation in a column.

#figure(
  spreadsheet(
    columns: 6,
    [], [], [], [], [], [],
    [], [], [], [`SCR(id, "Total", 0)`], [], [`Constant(0.015)`],
    [], [`SCR(id, "Interest", 1)`], [`SCR(id, "Deposit", 1)`], [`SCR(id, "Total", 1)`], [], [],
    [], [`SCR(id, "Interest", 2)`], [`SCR(id, "Deposit", 2)`], [`SCR(id, "Total", 2)`], [], [],
    [], [`SCR(id, "Interest", 3)`], [`SCR(id, "Deposit", 3)`], [`SCR(id, "Total", 3)`], [], [],
    [], [`SCR(id, "Interest", 4)`], [`SCR(id, "Deposit", 4)`], [`SCR(id, "Total", 4)`], [], [],
    [], [`SCR(id, "Interest", 5)`], [`SCR(id, "Deposit", 5)`], [`SCR(id, "Total", 5)`], [], [],
    [], [`SCR(id, "Interest", 6)`], [`SCR(id, "Deposit", 6)`], [`SCR(id, "Total", 6)`)], [], [],
  ),
  caption: [An example of the from @chain:example1 converted to ],
  supplement: "Spreadsheet",
  kind: "spreadsheet",
  placement: auto
)<sps:embedded-structure>

=== Grid to Graph
In order to go from the _Compute Grid_ to the _Compute Graph_, we use an algorithm not unlike the conversion of the _Structure Model_ to the _Compute Grid_. We begin with the outputs and look at the compute unit tree in those cells. Then, for every reference we encounter in that tree, we link the _Compute Unit_ in the referenced cell to the tree. As such, this is done in a recursive manner. In case of the range reference, we separately link all the roots of the cells in the range.

=== Replacing References
While we already cover a lot with the inserting of the references in the compute grid, one reference remains broken: the range reference. When the range reference is compiled to the compute graph, we store all the compute units of the cells in the range as dependencies. When we compile this to code, we get a list of all the dependencies. If the _Range_ references a random range in the graph, that is exactly what we want. However, when the _Range_ references the column of a structure, we actually want it to be a special reference.

As such, the _Replace References_ compiler step converts range references that reference a column or other reference of a structure to a _Structure Column Reference_. We traverse the graph and for every node, we look at its dependencies. If the dependencies are multiple _Structure Cell References_, we check if these cell references cover the whole column. If they do, we convert these _Structure Cell References_ to a single _Structure Column Reference_.

More importantly, this pass also introduces the _creation_ of the structures. Until now, we have always assumed that structures always existed. However, when compiling this to code, we need to instantiate this structure. As such, we create a _Structure Creation_ Compute Unit.  Every reference to a structure will have a dependency on the creation of that structure. 

In this pass, we also introduce the dependencies of the _Structure Creation_ itself. The dependencies are the data of the structure, stored in the _Structure Data_ object. The calculations for this data is also inserted into the Compute Graph, taking existing nodes into account.

= Coding the Structures<sec:code-model:changes>

Once the _Compute Model_ has been enriched with the structures and the _Compute Graph_ has been optimised to support the structures, we can output the structures and make the code more readable. The _Code Model_ is already expressive enough to support this new addition to the _Code Phase_, so we only need some changes to the compilation flow. 

First, we need to create the actual types and constructors, given the _Structures_ and _Structure Data_ respectively. Then, the newly created _Classes_ need an optimisation to handle mutual recursion that otherwise significantly slows the compiled code. In this section, we will discuss these changes.

== Creating types from structures

In the _Compute Model_, structures were separately stored. In the _Code Model_, they are still stored separately, but will be compiled to a _Class_. This _Class_ will be instantiated in the main code, and operations will be called to calculate values in the _Structure_. In this subsection, we quickly discuss what types are created for the _Table_ and _Chain_ types.

#figure(
  spreadsheet(
      columns: 5,
      [], [], [], [], [],
      [*Montly Expenses*], [], [], [], [],
      [], [], [], [], [],
      [*Description*], [*Category*], [*Projected*], [*Actual*], [*Difference*],
      [Extra... activities], [Children], [40.00], [40.00], [=D2-C2],
      [Medical], [Children], [], [], [=D3-C3],
      [School supplies], [Children], [], [], [=D4-C4],
      [School tuition], [Children], [100.00], [100.00], [=D5-C5],
      [_TOTAL_], [], [], [], [=SUM(E5:E8)],
    ),
    kind: "spreadsheet",
    supplement: "Spreadsheet",
    caption: [The _Structure Model_ of a spreadsheet containing a _Table_ describing the monthly expenses. Taking `E9` as output, the `Description` and `Category` columns will be pruned.]
)<sps:types:table>

=== Tables
Since a table is defined as independent rows, the _Code Model_ sees it as a list of row data. This row data is transformed to a 'table item' class. All columns are present as properties of the class. The data columns are normal properties, and the computed columns will become computed properties. The _Table_ itself is represented as a list of that 'table item'.

Take @sps:types:table for instance. This _Table_ will contain three columns when converted to the _Compute Model_: the two data columns `Projected` and `Actual` and the computed column `Difference`. Compiling this _Table_ to the _Code Model_ will result in a new type or class called `MonthlyExpensesItem` containing the properties `ActualCost`, `ProjectedCost`, and the computed property `Difference` which just calculates the difference depending on the other properties.

#figure(
  ```cs
  class MonthlyExpensesItem 
  {
      public double ProjectedCost { get; set; }
      public double ActualCost { get; set; }
      public double Difference => ProjectedCost - ActualCost;
      
      public MonthlyExpensesItem(double projectedCost, double actualCost)
      {
          ProjectedCost = projectedCost;
          ActualCost = actualCost;
      }
  }
  ```,
  caption: [The compiled code of the _Table_ in @sps:types:table. The `Difference` computed column is created as a computed property.]
)

The actual table is represented by the list of all items: `List<MonthlyExpensesItem>`. Operations on the columns of the table will be done through mapping operations, employing LINQ in C\#. For instance, to get the sum of all the differences in the `monthlyExpenses` table, we say `monthlyExpenses.Sum(m => m.Difference)`. In other languages, this would have nearly the same syntax, such as the map operation in Kotlin: `monthlyExpenses.sumOf { it.Difference }`.

The construction of the _Table_ is done through the creation of a list, with creation of the individual items representing the rows. For the example above, the constructor would look like this:

  ```cs
  List<MonthlyExpensesItem> = [
    new MonthlyExpensesItem(100, 105),
    new MonthlyExpensesItem(90, 50),
    ...
  ];
  ```

  When a structure is not marked as input, this constructor is called in the main function. The data is sourced from the _Table Data_.

=== Chains

Chains are a bit more complicated than Tables. Because the rows are depending on each other, we cannot exercise the same strategy as with the tables. As such, we represent the chain as its own class. Data columns are stored column-oriented instead of row-oriented. Computed and Recursive columns are calculated on an individual basis: In order to calculate a value in a cell in a computed or recursive column, we call the function `{ColumnName}At(x)`. 

For instance, the savings chain in the budget example is compiled to the following code:

#figure(
```cs
public class Savings
{
    public List<double> Deposit { get; set; }

    public double TotalBaseCase {get; set;}

    public double InterestAt(int counter) => 0.015 / 12 * TotalAt(counter - 1);
    
    public double TotalAt(int counter)
    {
        if (counter == 0) return TotalBaseCase;
        return TotalAt(counter - 1) + InterestAt(counter) + Deposit[counter - 1];
    }

    public Savings(List<double> deposit, double totalBaseCase)
    {
        Deposit = deposit;
        TotalBaseCase = totalBaseCase;
    }
}
```,
caption: [A code snippet of the Savings chain compiled to C\# code.]
)<code:chain:compiled>

The _Deposit_ column is compiled to a list of doubles. Furthermore, the computed and recursive columns are compiled to the `InterestAt` and `TotalAt` properties. Notice that the `TotalAt` property contains a recursive call to itself, and thus requires a base case. The computed column _Interest_ does not require a base case since it does not have a reference to itself.

The construction of the _Chain_ is done differently than the table. Instead of creating individual items, we pass each column along as input:

```cs
Savings savings = new Savings([500, 550, 450], 10_000)
```

When a structure is not marked as input, their constructor is called in the main function. The data is sourced from the _Structure Data_.

== Optimisations for Mutual Recursion

Running the emitted code that includes the chain @code:chain:compiled without optimisations leads to a very long runtime. It appears that the conversion to imperative code introduced an unwanted performance hit. Upon closer inspection, this is due to the recursive nature of the chain.

The two recursive definitions call each other and as such, the functions have _Mutual Recursion_. In the example, we see that the `TotalAt` calls itself _and_ the `InterestAt` function. However, the `InterestAt` function also calls the `TotalAt` function. This means that one call to the `TotalAt` call creates two extra calls to `TotalAt`. Hence, the amount of times `TotalAt` is called equals $2^x$ where $x$ is the input to the function.

There are some ways to fix mutual recursion, one of which is to transform mutual recursion to single recursion @kaser_conversion_1993. This is done through inlining of one of the methods. However, this is not really desirable, since it will lead to duplicate code, and that is exactly what we are trying to avoid.

Instead, we use memoization. Memoization is the practice of storing the input and output of a pure function as a pair @michie_memo_1968. When a function is executed, the input is checked against that pair, and if there is a matching pair that output is returned, saving on the computation costs and avoiding the spawning of further recursive calls @michie_memo_1968. On the first call with a new input, the output is calculated and added to the list of pairs. 

We do this with the mutual recursion in @code:chain:compiled as well. @code:chain:optimized shows the new mutual recursion solution. Every function that spawns two or more function calls to itself (direct or indirect) will be memoized. As such, `InterestAt` will not use memoization, but `TotalAt` will. This speeds up the execution by orders of magnitude.

#figure(
```cs
public class Savings
{
    private readonly Dictionary<int,double> _totalAtMemoization = new();
    
    public List<double> Deposit { get; set; }

    public double InterestAt(int counter) => 0.015 / 12 * TotalAt(counter - 1);
    
    public double TotalAt(int counter)
    {
        if (_totalAtMemoization.ContainsKey(counter))
            return _totalAtMemoization[key];

        if (counter == 0) return 10000;

        double result = TotalAt(counter - 1) + InterestAt(counter - 0) + Deposit[counter - 1];
        
        _totalAtMemoization.Add(key, result);
        return result;
    }

    public Savings(List<double> deposit)
    {
        Deposit = deposit;
    }
}
```,
caption: [A simplified code snippet of the Savings chain compiled to C\# code.],
placement: auto
)<code:chain:optimized>

= Discussion of the Compiler Design<sec:excelerate:discussion>

During development of the compiler, several design decisions shaped the pipeline that is presented in the previous and this chapter. While we have already deliberated the design choices on the existing architecture, some choices that were made were hidden or did not fit the narrative of the sections. In this section, we discuss these decisions.

== Data Model

The original architecture contained a dedicated _Data Model_ and _Data Phase_ that would run parallel to the _Compute Phase_. The _Data Model_ would describe the concrete values that resided in the structures and was based on the data layout dialect in MLIR @lattner_mlir_2021. The original idea was that this would cause complete separation between data and compute. However, this layer proved counter-productive for several reasons.

First, the _Data Model_ prevents values from being computed. Because of the complete separation, values cannot be represented by a _Compute Unit_. As such, it was impossible to refer to another cell from within a data column, since the _Data Model_ could not understand this. This was especially problematic in chains, since they often contain references in their base cases. 

Furthermore, the new _Structure Data_ concept provides the same descriptive power, including computation, while still separating the data from the definition of the structure. 

Finally, the removal of the _Data Model_ reduced the compilation flow from four phases---with two running in parallel---to a much simpler three-phase sequence. It also means that there are no more mutual dependencies between two compiler phases. This considerably lowers the cognitive load and improves maintainability.

== Computed Properties

The current implementation of structures and their computed properties was implemented in a simple way. This is effective for most use-cases, but introduces some limitations on the compiler: A computed property cannot have an external input as reference. A computed property is currently evaluated in isolation of the global scope. This means that the input is not available and as such, the emitted code will not compile. This represents a potential threat to validity that could be addressed in a future redesign of structures.

A possible redesign would be to rethink the structure design. Each externally referenced cell would be collected and stored seprately. When emitting the code, the classes representing the structure would include these external references as properties of the class and they would get their value from constructor parameters.

This poses a limitation on the list, since it is not a class and cannot add Instead of a list with items, the table would be its own type, just like the chain. Once we have parity, we can introduce these new properties. Due to limited time, this more robust system was not implemented. 

==== Merging structures 
Due to this limitation in the current compiler, it is not possible to reference data in another structure. This enforces isolation between structures. As a result, when a table 'shares' a column through formula dependencies with another column, the compiler will not compile the spreadsheet correctly. 