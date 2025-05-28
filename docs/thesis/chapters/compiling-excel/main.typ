#import "../../utils/chapters.typ": chapter

#chapter("Compiling Excel")

#include "compiler-structure.typ"

#include "ahead-of-time-compilation.typ"

// NEW //

= Trivial Flow
In the most basic sense, an Excel compiler can be constructed using two steps: extracting the formulae and their dependencies. Like we discussed, the underlying computational model of Excel is purely powered by the  

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

#pagebreak()



= The Trivial Route
The most trivial way to compile an Excel workbook is by extracting the underlying computational model and compiling it directly to C\# code. The underlying computational model is comprised of a graph of connected formulae, where formulae in different cells are connected through the use of references.
In this section, we briefly discuss the easiest way to compile an excel sheet and evaluate the output. As we will see in the rest of the thesis, we can use a different, more robust form of _structural compilation_ to compile the spreadsheet better.

#include "examples/budget-income.typ"

We walk through the example of a portion of the budget spreadsheet, shown in @sps:budget:income. We see it is a table that describes the three incomes of the family, with the expected (projected) and the actual amount of money that has been received this month. This is a basic example since there are not that many complexities in this spreadsheet. However, we will see that the simple compiler cannot even compile this simple example.

== Compute Units
For simplicities sake in this section, we will assume we already can easily access the contents of the cells. The first step or compiler pass of the trivial compiler is extracting the compute flow graph. However, unlike in other languages, in Excel has no explicit control flow, except for certain formulas. 

Hence, we construct a Compute Graph (which is similar but distinct from a compute flow graph), where we consider the contents, functions and operators to be nodes in the graph. We call the nodes in the graph _Compute Units_. For instance, if we look at a different view of @sps:budget:income we see the formulae that computes the cell `D5`. If we take this as an output for our Excel compiler, the 

Before we can generate code, we need to extract the different IRs 

- Explain the most basic form of the compute unit, with only functions and constants.

== Missing Structure
- Describe what is missing from this route, and how we need _structural compilation_.

= Structural Compilation

== Structures

=== Tables

=== Chain-Tables

=== Repositories

== One or more IRs



= Modeling the workbook

== Reading the Excel File

== Extracting Areas & Implicit Structures

== Formal definition of the Structure IR

= Deriving the Logic

== Compute Grid

== Compute Graph

== Type Resolution

= Extracting the Data

== Variable Separation

== The Data IR

= Generating Readable Code

== Layout and Emission

= Discussion of the Compiler Design