= Excel

Excel is an (online) spreadsheet application developed by Microsoft. It is used extensively in the world of Finance, as can be inferred by the sheer amount of financial-themed books and papers on the topic. It allows for data modelling, manipulation and analysis, transforming a painful manual calculation into an automated process in just a few clicks. 

In this subsection, we cover the basic principles of Excel necessary to understand this thesis. First, we discuss the structure of an _Excel Workbook_. Then, we look at Excel formulae. Finally, we give a high-level overview of the calculations in Excel.

== Excel Structure

All information of an Excel file is stored in an _Excel Workbook_. The workbook is the aggregate of all excel entities, containing standard styles, file configurations, preferences, and most importantly, the _Excel Worksheets_.

Historically, the _Excel Workbook_ file has been housed under the `.xls` extension. However, since Excel 2007 the Excel workbook is saved as an `.xlsx` file: a zip compressed folder comprised of XML files. These files contain information on metadata, or contain the actual data and formulae of the worksheets.

The real data is stored in the _Excel Worksheet_. This spreadsheet is a two-by-two grid of cells, which can be filled with values. The values in a cell can also be calculated, but the user will only see the value unless clicking on the cell. Besides the cell-strcuture, the Excel worksheet can also contain graphs and images. Furthermore, the cells can be styled in a certain format, and will follow the standard style of the workbook by default. That said, in this thesis, we mostly look at the data and the computations on the data.

Excel also has other _special structures_. These structures live inside the grid, but augment the functionality of excel, simplifying certain operations.

=== Example: Family Budget
#figure(
  image("../../images/family-budget/overview.png", width: 70%),
  caption: [The _Monthly Budget Report_ overview spreadsheet of the _Family Budget_ workbook.], placement: auto
)<fig:family-budget:overview>

A good example is the _Family Budget_ workbook, which mimics a budgeting application. In this workbook, we can see three are three _worksheets_, namely the _Monthly budget report_ (which is selected and is visible in @fig:family-budget:overview), _Monthly expenses_, and _Savings_. @fig:family-budget:overview also shows the styling that is possible in the Excel sheet. This styling is also saved in the workbook.

=== Tables#footnote[https://support.microsoft.com/en-us/office/overview-of-excel-tables-7ab0bb7d-3a9e-4b56-a3c9-6c94334e492c]<subsec:excel-overview:tables>

_Excel Tables_ elevate normal tables by introducing functionality unique to Excel Tables. The table structure is arguably the most common structure in Excel, where a table is created with columns of a certain type, and rows filled with information. With Excel tables, it is possible to formalize these notions, and explicitly define columns and the size of the table.

_Excel Tables_ have a name. Their columns also have a name. It is possible to use these names in a _structured reference_, where instead of defining the range of the column like `A1:A50`, a more semantic notation `tableName[columnName]`  is used. This notation is just like indexing an array or a dictionary, and provides more readable formulae.

There are two types of columns in an Excel table. Columns can have arbitrary data and computations, or have a fixed, computed formula for all rows in the column. This formula can reference other columns using a special notation. For example, a column can reference other columns by using the `@` notation in a _structured reference_, resulting in something like `@[column1] + @[column2] * $A$1`. Every row in the table will then update the column with that formula, and the `@`-notation will automatically target the cell in the row and specified column.

#figure(
  image("../../images/family-budget/monthly-expenses.png", width: 70%),
  caption: [The _Monthly expenses_ spreadsheet in the _Family Budget_ workbook: A table with five columns describing the columns. The _Difference_ column is computed from the actual cost and projected cost column.],
  placement: auto
)<fig:family-budget:monthly-expenses>

For example, in @fig:family-budget:monthly-expenses the monthly expenses table is a designated table with five columns. The table has four data columns, and one computed column. In the difference column, the formula in the upper row is `Monthly expenses'!$E5-'Monthly expenses'!$F5`. In the background, Excel recognizes the dependencies in this formula and automatically applies transformations to make it work in the other rows. So for example, in row seven, the formula is changed to `Monthly expenses'!$E5-'Monthly expenses'!$F5`. 

=== PivotTables #footnote[https://support.microsoft.com/en-us/office/overview-of-pivottables-and-pivotcharts-527c8fa3-02c0-445a-a2db-7794676bce96]

The _Excel PivotTable_ is a powerful tool for summarizing and analyzing data from external sources. Unlike normal tables, _PivotTables_ are dynamic in layout, and allow for reorganization of the data. It provides a dynamic view of the underlying datasource and allows the user to perform operations on the data. The core feature of _PivotTables_ lies in their ability to perform aggregations as a low-code solution. Without writing a single formula, it is possible to construct comprehensive views of the data. This is made even easier with automatic refreshing, which updates the PivotTable when there is a change in the underlying data model.

_PivotTables_ support grouping. Dates, for example, can be grouped into months or years, and numeric data can be binned into intervals. These groupings are handled internally and allow for a more compact and meaningful summary.

It is important to note that PivotTables do not compute data themselves. Instead, they act as a kind of presentation layer of other datasources. These datasources can be internal or external. Hence, while they could depend on the data and formulas defined elsewhere, they do not themselves perform cell-level computations in the same way as formulas in worksheets or structured references in _Excel Tables_.

== Formulae #footnote[https://support.microsoft.com/en-us/office/overview-of-formulas-in-excel-ecfdc708-9162-49e8-b993-c311f47ca173]

In Excel, a formula is an expression that calculates the value of a cell. Every formula begins with an equal sign `=`, which tells Excel that the contents of the cell constitute a formula. These formulas can contain functions, references, operators, and constants to perform calculations on data within the Excel sheet. These distinct elements all contribute different functionality to the end results:

- Constants are fixed values entered directly into the formula.
- Functions are predefined operations that perform specific calculations on their arguments. For example, `SUM(1;2;3;4;5)` will sum up the arguments. An interesting thing with Excel is that the names and notation of the functions are language dependent. For instance, in Dutch, the `SUM` function is translated to `SOM`. As well as the notation for the arguments list, where in one language the arguments are split with a semi-colon `;` , another language splits them with a comma `,`.
- Operators are special functions that can be inlined. For example, the plus `+` or minus `-` operators. For the order of operations, Excel follows the PEMDAS rule.
- References provide pointers to other cells or ranges within the worksheet in the form of the _A1-notation_. This notation targets the rows and columns, where the columns are represented by the alfabet, and the rows by a number. For instance, the cell in the third column, and fourth row, would be `C4`. [Provide Example with different references] References can be _relative_ to the cell or _absolute_. _absolute references_ are started with a dollar sign `$`. It is possible to only make the row or column absolute, to create a _hybrid reference_.

// == Calculation-Mode
// While most of the underlying logic of Excel is not available, since Excel is a commercial project, there are no official sources that discuss the underlying working of Excel. However, 