#let citeauthor(ref) = cite(ref, form: "prose", style: "../ieee-short.csl")

= Methodology <methodology>
== Challenges in Research

This research faces several challenge mainly due the proprietary nature of Excel and thus modelling the semantics of the function execution. While some research has been conducted on spreadsheet semantics @bock_semantics_2020, it remains unclear if these models can fully represent Excel's functionality and do not describe the dynamic nature of Excels dynamic formulas. 

Circular dependencies introduce additional complexity, as the iterative calculation methods used by pension fund operators remain ambiguous. The exact semantics of the iterative calculation (fixed point iteration) in excel can vary, because it depends on what the initial values are. Since it is crucial to maintain consistency in the values, these semantics need to be further analyzed.

Dynamic formulas such as `IF`, `RAND`, and `TRANSPOSE` add another layer of difficulty, requiring the model to account for undefined values and conditional outputs. For instance, the `TRANSPOSE` function takes in a range of values, and transposes this range. The output of the function is consequently an array, and will _spill_ into the neighbouring cells. These cells can be used in further computations. However, when combined with an `IF`, the transpose might not have been executed, and thus the neighbouring cells have not been filled and the further computations will be different.

Additionally, validating the semantics is challenging due to the limited availability of relevant datasets, as will be discussed in @subsec:insufficient-data. We can dynamically compute some of the calculations using the EPPlus library @epplus_software_epplus_nodate, but they do not support circular references. Therefore, another method of validating the circular references is required.

== Expected Input from the Literature Survey

We expect the literature survey to have limited input on the thesis. The spreadsheet book by #citeauthor(<sestoft_spreadsheet_2006>) provides a foundation of the inner-workings of Spreadsheet programs, and the work of #citeauthor(<bock_semantics_2020>) offers a starting point for formalizing semantics, though further verification for Excel is required. The master theses discussing support graphs @iversen_runtime_2006 @poulsen_optimized_2007 will help the development of the first mapping to an internal domain model.
//Research on program generation will guide the translation of Excel functionality into high-level code.

== Experimental Design and Proof of Concept

The research will compute mappings between Excel functions and high-level code in the .NET framework (C\# language). This includes designing a domain model that handles cyclic dependencies and dynamic spreadsheet behavior using iterative calculation techniques. We will develop a prototype to demonstrate the feasibility of the approach, focusing on accurately converting selected Excel formulas into human-readable .NET code. Once the initial domain model is constructed, testing will evaluate performance, correctness, and capabilities. We will examine the formal semantics of the excel formulas and compare them to the .NET mappings.

== Research Method and Validation
Since there is not many research on the area of mapping excel formulas to code, this thesis describes an exploratory study with design elements.

A mapping will be designed that will map Excel functions to an executable, readable code listing in the .NET framework. Within this mapping, we will design a model that works with circular references and dynamic functions in Excel.

A quantitative approach will be used to evaluate the proof of concept, focusing on metrics such as execution time in seconds, memory usage in MiB, and transformation success rates. 

We will verify the code readability using software metrics like formal verification using the natural semantics @kahn_natural_1987. Besides, Empirical validation will test the model against real-world datasets, comparing outputs to Excel, and validating edge cases in complex formulas. Iterative refinement will ensure the prototype evolves to meet the research objectives.

== Hypotheses

=== Does there exist a human-readable high-level code representation of complex compositions of excel formulas?
We hypothesize there exists a human-readable representation of most formulas. However, we do believe there are certain restrictions on the compilation, especially when it comes to the complex dynamic formulas.

==== Does there exist a mapping for Dynamic Excel formulas?
Our hypothesis is that a mapping for dynamic formulas will exist, but with restrictions. In highly complex scenarios, where multiple dynamic formulas are being used, compilation to normal imperative code may be difficult due to their use of the spreadsheet datastructure (something we are trying to remove).

==== Does there exist a mapping for cyclic references?


==== What are the differences between the Excel Formulas and the compiled code?

=== How can the mapping between excel formulas and code be verified?
We theorize the mapping can be verified using two ways: empirical and formal. For empirical verification, we hyptohesise it can be done, but will require a lot of test cases to cover the complexity and edge-cases of excel formulas.

For formal verification, we expect that we can model most of excel formulas. There will be some difficulty defining the formal semantics for the more complex formulas. We believe not the full semantics of the dynamic formulas can be mapped, due to their inherent complexities and their reliance on the spreadsheet datastructure.


#enum(numbering: n => strong[(RQ#n)], 
  [*How can complex compositions of excel formulas be mapped to human readable .NET code?*

  Our hypothesis is that by using an internal domain model inspired by the support graph @poulsen_optimized_2007, we can map all of the 'static' excel formulas like `SUM` and numeric operations, also with complex operations. We expect a difficulty in readability when the formulas become quite complex and large, since there will probably be a lot of duplicate code, reducing the readability. We expect it to be quite difficult to reduce this duplication, since the dependencies will vary in every cell.
] + enum(numbering: n => strong[(RQ1.#n)], indent: -15pt, 
  [*How can Dynamic Excel formulas like `TRANSPOSE`, `RANGE` and `INDEX` be represented in a support graph?*

  We hypothesise that the dynamic formulas like `TRANSPOSE` can be statically analysed to get a conditional path in the support graph.
  ],
    [*How can cyclic dependencies with iterative calculations be mapped to .NET code?*],
    [*What are the differences between the Excel Formulas and different versions of compiled code?*]
  ),
  [*How can the mapping between excel formulas and code be verified?*],
)<researchQuestions:subQuestions>

== Timeline

The research will begin by defining mappings and researching efficient storage mechanisms for function results. Iterative calculation techniques will be developed and integrated into the domain model. Testing will then evaluate performance and correctness, followed by iterative refinement to support dynamic formulas. Finally, a semantic model will be constructed, and formal verification will validate the compiler’s correctness. The final phase will involve comprehensive testing, documentation, and demonstration of the prototype. Please see @fig:gantt-chart for the full timeline.