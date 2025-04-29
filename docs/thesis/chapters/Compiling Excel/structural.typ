#import "@preview/zebraw:0.5.2": *
#show: zebraw.with(numbering: false, inset: (left: .6em), )


= Structural Model
This representation captures the exact essence of the whole Excel Workbook (or file) and allows for manipulation on cell level. This model preserves the grid layout of the spreadsheet. This IR acts as a foundational map for building higher-level representations, anchoring the subsequent computations to their visual and logical positions in the spreadsheet.


```cs
record Workbook(
    string Name, 
    Reference[] NamedReferences, 
    Spreadsheet[] Spreadsheets);

record Spreadsheet(string Name, Cell[,] Cells, Table[] Tables);

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
References are a way to refer to a cell, range or table. 