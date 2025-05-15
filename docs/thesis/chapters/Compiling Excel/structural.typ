#import "@preview/zebraw:0.5.2": *
#show: zebraw.with(numbering: false, inset: (left: .6em), )


= Structural Model
This representation captures the exact essence of the whole Excel Workbook (or file) and allows for manipulation on cell level. This model preserves the grid layout of the spreadsheet. This IR acts as a foundational map for building higher-level representations, anchoring the subsequent computations to their visual and logical positions in the spreadsheet.


```cs
record Workbook(
    string Name, 
    Reference[] NamedReferences, 
    Spreadsheet[] Spreadsheets);

record Spreadsheet(string Name, Set<Cell> Cells, Table[] Tables);

record Cell(Location Location);
  -> record ValueCell<T>(T Value);
  -> record FormulaCell(string Formula);

record Table(string Name, Range Location, Column[] Column);

record Reference();
  -> record Location(int Row, int Column, string Spreadsheet)
  -> record Range(Location From, Location To)
  -> record TableReference(string Table, string[] Columns)
```

== Workbook
The workbook is the grand model of the excel file, representing all the data. A workbook contains references to the spreadsheets that fill up the workbook.

In more mathematical terms, a Workbook $W$ can be seen as a three tuple $(N, S, R)$ where

- $N$ is the name of the workbook. While this is not needed for the correct compilation of the workbook, it is used for naming the final name-spaces in order to distinguish two different compiled spreadsheets.
- $S subset.eq SS$ represents the set of spreadsheets (sometimes called worksheets) and is a subset of the set of all possible spreadsheets $SS$.
- $R$ represents the set of named references that are globally present. These are mostly the available _Tables_ in the document.


== Spreadsheet
The excel spreadsheet $S$ can be represented as a four-tuple $(N, R_"named", C, T)$ where

- $N$ is the name of the spreadsheet, which can be referenced in the same or other spreadsheets.
- $R_"named"$ represents the set of named references that are specific to this spreadsheet.
- $C subset.eq CC$ represents the filled cells in the spreadsheet, where $CC$ represents the set of all possible cells.
- $T$ represents the set of tables in the spreadsheet.

The spreadsheet can be seen as a two-by-two grid of cells. This notation considers spreadsheets to be infinitely large. In practice, Excel does not allow spreadsheets to have more than 1,048,576 rows and 16,384 columns #footnote[#link("https://support.microsoft.com/en-us/office/excel-specifications-and-limits-1672b34d-7043-467e-8e27-269d656771c3", [By Excel Specification])]. 

=== Tables
Excel supports a built-in special structure of continuous neighboring cells called a Table. Tables are special in a way that they are ranges of cells that automatically have a name, and they can be indexed by the columns of the table. 

Tables allow special operations, such as filtering and aggregation on the data 

A table can be seen as a three-tuple $T = (N, R, C)$ where

- $N$ is the table's name.
- $R = (L_"from", L_"to")$ is the range that covers the whole table, from the top-left cell at $L_"from"$ to the bottom-right cell at $L_"to"$
- $C = {C_1, C_2, ..., C_n}$ is the set of all columns in the table.

Each column $C_i$ is itself a two-tuple $C = (N, R)$ where

- $N$ is the name of the column, if present. The name of the column is usually in the header of the table and describes the contents of the column. If there is no header present, there is no name.
- $R$ is the range of the column and does not include the header. The range only represents the data in this column.

== Cells
The cell is the atomic unit of data in a spreadsheet. It represents a constant value or computation depending on the content. 

We represent a cell $C$ as a two-tuple $C = (L, V)$ where

- $L$ is the location of the cell in the spreadsheet.
- $V$ is the value of the content in the cell.

Depending on the value of $V$, we assign a special status to the cell $C$. If the content is a raw value like numbers or text, we say that the cell is a _value cell_. If the content is a formula expression that starts with `=`, we call the cell a _formula cell_.

=== Formula Cells
Formula cells are special cells that contain formula and form the basis of the computational model and the transformation of the structural model to the compute model. 

The content of the formula cells are always plain strings. The formulae do not get parsed in any way and are stored in their raw form. This distinction allows us to keep the structural model separate from other models.

Still, semantically, it is important to still consider these formula cells apart from their computational, parsed form. In many cases, the value in a cell represent an important (part of a) calculation. [Create an example here]

=== Formatting
It is possible to use custom formatting for a cell in Excel. For instance, a number $4.00$
can be formatted to let it look like a currency $euro 4.00$. It is often used with dates. 

We do not consider formatting primarily because Excel stores the content of a cell apart from the formatted value. Hence, we can always take the 'normalized' value and use these in our calculations. This avoids the conversion of formatted values to normalized values.

== References
References are a way to refer to a cell, range or table. They link computations together by representing the (computed) value in another computation.

// One way to look at a reference is in a _singular_ form. This means that the reference B3 is essentially the same as B3:B3, which denotes a range from cell B3 to cell B3. Semantically, they are the same as they both point to the same cell. 

We distinguish between references to cells like `B3`, references to ranges like `A1:B3`, and references to tables like `Table[ColumnName]`. The distinguish between these references since they contain information that we can use in further compilation. Take the cell B3 for instance. This cell reference is essentially the same as B3:B3 as they both point to the same cell. However, the B3 cell reference is conceptually different than B3:B3 in programming terms, as the cell reference is just a single value, and the B3:B3 is a singular array.

Formally, we define references as the discriminated union $"Ref" = "Ref"_"cell" | "Ref"_"range" | "Ref"_"table"$ where:

- $"Ref"_"cell" = (R, C, S)$ is a cell reference that references the cell in row $R$, column $C$ and spreadsheet $S$.
- $"Ref"_"range" = ("Ref"_"from", "Ref"_"to")$ is a reference to an area of cells, where $"Ref"_"from"$ is the reference to the upper left cell, and $"Ref"_"to"$ is the reference to the lower right cell. These two cells form a continuous matrix of cells.
- $"Ref"_"table" = (T, C)$ is a reference that references named column $C$ in table $T$. The table $T$ may be in any spreadsheet.

=== Data References
Cell and Range references are references that reference values and are addressing the structure of the spreadsheet. They cannot reference data that might not be in the spreadsheet yet. For instance, in the budget example, the list of expenses might be different from month to month. To support this, the excel compiler considers this data (especially in tables and chains at the moment) separately (which we talk about more in @sec:data-model). To reference this data, we use a data reference. 

The data reference $"Ref"_"data" = (R, N)$ is a reference to address $N$ in a data repository $R$. The address can be a column in a table, or a single cell reference.