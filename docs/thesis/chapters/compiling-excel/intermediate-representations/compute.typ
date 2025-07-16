= Compute Model
The compute IR models the underlying computational model of Excel, powered by Excel's Formula engine. This layer considers the whole Excel workbook and represents the computation beyond cells and worksheets. 

We consider the compute unit as one big control flow graph which encodes references, and computations. 

```cs
record ComputeUnit(Location Location, ComputeUnit[] Dependencies);
  -> record Nil();
  -> record ConstantValue<T>(T Value);
  -> record Reference();
    -> record TableReference();
    -> record CellReference();
    -> record RangeReference();
  -> record DataReference(); // Not the same as a normal reference.
  -> record Function(string Name);
  -> record Table(Column[] Columns);
  -> record Chain(Column[] Columns);
  -> record Choice(ComputeUnit selector);
record CircularDependency(ComputeUnit[] CircularDependencies)
```

```fs
type ComputeUnit = { location: Location, dependencies: ComputeUnit[], type: ComputeUnitType }

type ComputeUnitType = 
  | Nil
  | ConstantValue of Value
  | Reference of Reference
  | DataReference
  | Function Name
  | Table Columns
  | Chain Columns
  | Choice Selector

type Reference =
  | TableReference
  | CellReference
  | RangeReference
```

#show raw: set text(font: "JetBrains Mono")

#figure(
  ```
    ComputeUnit = (location: Location, dependencies: ComputeUnit[], type: Type)
    |- Nil
    |- ConstantValue += (value: Value) 
    |- Reference
       |- TableReference += (tableName: string, columnName: string)
       |- CellReference += (reference: Location)
       |- RangeReference += (from: Location, to: Location) 
    |- DataReference
    |- Function
  ```,
  
)

== Compute Unit
At the core of the compute model lies the compute unit. This unit represents a basic operation with input and output. Compute units can be connected to each other, forming a network or flow of computations. When compute unit _A_ uses the output of compute unit _B_ as input, then we say that unit _B_ is a _dependency_ of unit _A_. Conversely, unit _A_ is a _dependent_ of unit _B_.

Consequently, we represent a basic compute unit as $C U = (L, D_"in", D_"out", T)$ where

- $L$ is the location of the compute unit in the cell grid in the structural representation. 
- $D_"in"$ are the dependencies or input of the cell.
- $D_"out"$ are the dependents of the cell.
- $T$ is the type of the output of the calculation

The network of compute units can be represented as a graph of nodes. This graph is called the _Support Graph_ @sestoft_spreadsheet_2006 and models the full computation done in Excel. See @subsec:support-graph for more information on the support graph.

In the next couple of sections, we will elaborate on the different _kinds_ of compute unit, discussing their existence.

== Constant Values
Since every computation and compute unit needs inputs, there need to be nodes that do not contain any dependencies. These nodes are the _constant values_ in the support graph. Constant value nodes only contain a value and an accompanying type.

A special constant value is the value that represents nothing: the _nil_ node While this node will not be emitted to any subsequent intermediate representations, it provides a way to represent an empty cell within a calculations. In subsequent compiler passes, these empty cells will often be removed.

== Reference
Within the compute representation, it is still possible to reference other values. While it is desirable to have no references in the support graph at the end of all the compiler steps, having references can heavily simplify compiler design. Just like in the structural representation, we distinguish between three different kinds of references: _cell_, _range_, and _table_.

A different kind of reference is the data reference. As we will discuss in @sec:data-model, the data and compute model are closely collaborating within the compiler. For computations to work, the data in these repositories should be addressable. This is possible through the _data reference_, which is essentially a  tuple $"ref"_"data" = (N_"repo", L)$ where $N_"repo"$ is the unique name of the repository, and $L$ is the location within the repository to reference, which may be a range or a single cell.

== Compute
In order to actually compute data, Excel uses formulas. These formulae and compositions thereof are converted to a graph of _Functions_. In this representation, functions are only represented in their signature form, without their implementation. We leave this part to the code-layout representation and a single compiler step which converts the functions to their respective code representation.

== Special Constructs
Besides from the _primitive_ constructs in the compute model, we also distinguish special, Excel-specific constructs. These constructs are common patterns found in many Excel sheets. Most of these patterns can be mapped to patterns in code in order to make code more readable. In the next subsections, we discuss these patterns.

=== Table

Like discussed in the previous section, a common structure found in Excel sheets is the table. This construct is continuous region of cells, commonly with column- or row-headers. 

In sense of the computation, a table will always have some sort of computed column. This column depends on values in the same row from other columns. For instance, when computing the difference between the projected and actual costs in the Monthly Expenses table, the column 'difference' depends on the 'projected' and 'actual' column. In mathematical terms: $ forall t in "Tables", c in "Cells"(t). "Formula"(c) => forall d in "dep"(c). "Row"(t, c) = "Row"(t, d) $

#let colDeps = $"dep"_"cols"$

The columns of a table have to have the same calculation. All calculations in that column should refer to the same columns. In mathematical terms:

$ forall t in "Tables", \c\o\l in "Columns"_"Computed" (c). forall c_1, c_2 in \c\o\l. colDeps(c_1) = colDeps(c_2) $

where #colDeps is the columns of the dependencies.

=== Chain

The chain is similar to the table. However, the chain has some of the requirements of the table relaxed. A chain is a continuous region of cells like the table where some of the columns have calculations that have dependencies on columns in _other rows_. This important distinction means that the calculation in the region is a long sequence of calculation involving information found in the table, like a _chain_ (hence the name).

Because the chain depends on the previous row(s), we distinguish certain initial values, which can be constant values, references to data or different formulae not depending on values in other rows.

For example, consider a savings account spreadsheet. Every year the savings account will get more interest, and the amount of interest will grow. The columns contain the amount of money currently in the savings account, the amount of interest that year, and the potential money you add to the savings account during the year. Here we can see that the column for interest and the actual amount of money in the savings account is the computed column part of the chain. This chain calculates its value by taking the value of the interest the previous value of the money in the savings account And also the money. that you are putting into them savings account and then calculating the interest you had on this and updating the value in the current row.

=== Choice 
Excel also has flow control functions such as sum if or choice. Functions can have simple constant values in them, or references to other functions, which means that they split the path of computation.

We model this choice with its own choice node. This choice node has a. selector function which can select the path of computation to do next. within the actuarial computations, we see that there are a lot of different paths to take, and thus the choice operator is really important in these calculations.

== Support Graph<subsec:support-graph>
The support graph is a multi-directional cyclic graph that describes the underlying compute model of an Excel sheet. The support graph used in this thesis is different than those found in Excel or in @sestoft_spreadsheet_2006. This is mainly due to the fact that this graph is multi-directional and can thus be traversed from both sides, which heavily simplifies compilation.

The support graph supports the operation for traversing the graph. This is done in topologically sorted way, which means that when we traverse node _a_, we have already traversed the dependencies of _a_. This ensures consistent traversal and updating of the graph.
Traversing the graph and making updates to the graph is a common operation within a compiler step.

=== Circular Dependencies
The circular dependency is an important construct. Within the support graph, it is possible to have circular dependencies. However, when traversing this support graph in topological order, we want to quickly determine the circular dependencies. Furthermore, in later stages, we want to create a special mechanism to determine the value of the computations. Hence, we store the circular dependency separately by storing all the nodes that are in the circular dependency.

The circular dependency thus creates a sub-graph of the support graph. When traversing the support graph, the dependencies of the circular dependency will first be presented, then the circular dependency itself, and then the dependents of the circular dependencies.

== Types
In Excel, different types exist, from booleans, numbers to dates and arrays. In order to fully model the computation, these types should be included. Types are very important, especially when trying to refactor existing code. Without types, you could end up with an operation that is not possible on a certain value because of a type mismatch. This would make it impossible to compile this model to the code layout model.

Besides primitive types that also can be found in Excel, it is important to consider the special structures we have just created. A lot of these special structures can be mapped to a complex type or a class, where we can use methods and properties to represent calculations. 

For example, the _Monthly Expenses_ table contains the data columns _Actual_ and _Projected_, but also a computed column called _Difference_. This can be mapped to a datatype, where _Actual_ and _Projected_ are simple fields, and _Difference_ is a property that uses the _Actual_ and _Projected_ fields.

=== Precision
Precision is key, especially in the actuarial calculations context. We don't want to lose a few decimal due to precision issues, which would propagate and mean that the pension fund would be paying more or---even worse---less than what you should have gotten. 

Within Excel, most datatypes adhere to a 15 digit precision. This precision is derived from the IEEE 754 specification, which can only provide 15 digits of significant precision. Hence, we are safe to use the double precision floating data type. However, since we are talking about actuarial computations, it is actually better to have more precision. Hence, we model all fractional, floating numbers with quadruple precision floating points.
