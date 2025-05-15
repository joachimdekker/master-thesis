#import "../../utils/chapters.typ": chapter

#chapter("Compiling Excel")

= Compiler Structure
Just like any other compilers, the excel compiler consists of tiny steps that all contribute towards the common goal: readable C\# code. In between these steps, the code is represented using different languages. The languages or _intermediary representations_ (IR) help abstract the details of the excel code and structure.

== One or many IRs
Within compiler construction, we can take as many steps as we want. Sometimes, it suffices to directly compile from one source to the other source, such as in simple DSLs. That said, it is often easier to create an intermediary representation so transformations on the AST are much easier. But how many representations are the best fit? In the following subsections, we discuss the advantages and disadvantages of having many intermediary representations.

=== One IR

Using a single IR simplifies the architecture and makes the compiler pipeline easier to reason about. With one representation, all transformation steps happen in a unified format, and the state of the compilation is centralized.

However, a monolithic IR must serve too many purposes—capturing structure, logic, and target code layout simultaneously—which often leads to a bloated and inconsistent format. It becomes harder to maintain and more challenging to distinguish between high-level intent and low-level mechanics.

=== Many IRs

In contrast, using multiple IRs introduces overhead and boilerplate, but allows for separation of concerns. Here, each IR can focus on one specific aspect of the compilation process, such as the logical structure, data flow, and code generation. This focus makes it easier to reason and permits modular, small transformations generating readable code. 

Having many IRs also mean more transformation steps. Furthermore, on the one hand, it makes debugging harder since many information has to be tracked across the models. That said, we can apply assertion at the end of each 'layer' to make sure the model is up to standard before transforming it to the next model.

=== Excel Compiler
For the Excel compiler, we use multiple intermediate representations but rely on references between them to keep them interconnected. In the next sections, we introduce the four different IRs—Structure, Compute, Data, and Code Layout—and explore the rationale behind their existence and their internal design.

= High Level Overview
Before we dive deeper into the intermediate representations, it is helpful to discuss the high level overview of the compiler. This section, we walk through the biggest steps of the compiler and relate the different steps and structural models to each other. In subsequent sections, we dive deeper in the actual structure of all the models. 

== Budget example
Actual actuarial spreadsheets can be enormous. To ground the ideas, we introduce the _family budget_ example spreadsheet. This spreadsheet is used for keeping track of the family budget and is provided by Microsoft as an introduction to the features of Excel. This makes it an excellent example and benchmark for the features of the compiler.

While actually compiling this spreadsheet might be overkill, it provides a good way to think about the computations and help visualize the ideas presented in this thesis. We will introduce the various components of the budget example along the way. 

== Structure-aware compilation

Unlike traditional compilation, where trivia like whitespace and comments are often discarded [source?], this compiler heavily relies on the structure of the spreadsheet. The spreadsheet contains a lot of data that can be used in the compilation, be it in the form of how cells are populated: table-like or randomly. Spreadsheets contain common patterns that can be abstracted, such as tables with- or without interdependent rows.

In traditional compilation, the result of the compilation is often used for byte code or code that a human does not read. This means that whitespace and comments can just be removed, as they often do not contain usable information. Furthermore, trivia in the AST can be difficult to work with, and hence it is trivially removed [source?]. 

We call this form of compilation _structure-aware compilation_, where the compiler uses the structure of the source language as context in the compilation passes in order to improve the legibility of the code. Semantically similar spreadsheets that are different in layout will therefore likely result in different outputs. 
[Insert example?]
Within _structure-aware compilation_, we always maintain an abstracted representation of the structure of the source language. This abstracted representation is meant for use in all subsequent passes. Compilation of a source language will thus always begin with the extraction of the abstracted representation of the structure.

In the context of Excel, the structure-aware compilation introduces the _Structural Model_, which partially maps the Excel Workbook. It is important to note that this structure-aware compilation is different from the underlying compute model, which is being modelled by a different intermediary representation.

== Ahead of Time compilation
[This thesis is a form of Ahead of Time compilation, since we compile the excel compilation to another form. This AoT is pretty hard with dynamic arrays etc.]

#figure(
  table(
    columns: 4,
    [], [Projected], [Actual], [Difference],
    [Income 1], [\$ 6.000], [\$ 5.800], [-\$ 200],
    [Income 2], [\$ 1.000], [\$ 2.300], [\$ 1.300],
    [Extra Income], [\$ 2.500], [\$ 1.500], [-\$ 1.000],
    [TOTAL], [], [], [\$ 100]
  ),
  caption: [The Income Summary section of the Family Budget spreadsheet. The project and actual income are raw inputs, and the cells in the difference column calculate the difference between the projected and actual income.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:budget:income>

== Compilation Flow
Let's start with a short example of a very simple excel sheet. The budget spreadsheet contains a section for defining the income of the family. See @sps:budget:income for a visual representation. Say that we want to calculate the lower-left cell, which is a total of all the differences.

The first step is constructing the _Structural Model_, which is done by extracting the structure out of the Excel sheet. It is basically making a copy of the excel sheet into a format that is easier to work with. In the structural model, we also have a few passes that enrich the current model. For instance, the spreadsheet in the income does not explicitly define a table, but we can clearly see that this is an implicit table. Hence, passes like this add the tables and so on to the structural model where it can be used by subsequent steps.

From this structural model, we extract two things. First, we extract the computational flow of the excel sheet. The computational flow is the underlying computational model that calculates the cell and is powered by excel formulae. In reality, this model calculates every cell, but the compiler only gathers the relevant cells and formulae that calculate the final set of outputs. In the _Income_ example, the computational model that is gather is the summation of the differences, and the way the differences are calculated. Depending on the structure of the spreadsheet, the calculations may be simplified or de-duplicated.

We also extract data into their own representation from the structural model. This data layer is primarily to separate the data and computation. This is important in spreadsheets, as the data is often variable. Take the budget example where the projected and actual income might change every month. To distinguish certain constant values from the variable data, we provide an intermediary representation that stores this variable data and provides methods to provide data from outside sources, such as caches or databases.

The penultimate step is converting the previously mentioned computational model and data model to the code layout model. In this step, we leave behind the structural compilation, and fully focus on the code. In this last step, we implement a few compiler passes that simplify the code or increase the readability by splitting expressions.

Finally, the code is converted to the data model of the Roslyn Compiler Platform. This platform allows for programmatically emitting C\# source code.
 


#include "structural.typ"
#include "compute.typ"
#include "data.typ"
#include "code-layout.typ"