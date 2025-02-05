#let citeauthor(ref) = cite(ref, form: "prose", style: "../ieee-short.csl")

= Methodology <methodology>
== Challenges in Research
Creating a readable program from an Excel structure will be challenging. Excel is very loosely structured by nature, and when there are no descriptions associated with a cell to indicate what its value might represent, creating a semantic description for the entire compiled program becomes difficult.

Since Excel formulas need to be converted into object-oriented, imperative code, several challenges arise. The first challenge is extracting functions from the computations---that is, determining where to combine computations and where to split them into separate functions. There are many possibilities that need to be examined. Furthermore, scoping becomes an issue when building upon these functions. In a standard spreadsheet application, all cell computations are in scope; however, when using imperative code, reusing cached computations is nontrivial. A cell's value might be evaluated in one scope, but if it needs to be reused in another, sharing that value becomes challenging without resorting to global variables. This would hurt readability.

Circular dependencies add further complexity through mutual recursion. The exact semantics of Excel's iterative calculation (fixed point iteration) can vary, as they depend on the initial values. Since maintaining consistency in the values is crucial, these semantics need to be further analyzed. This could prove difficult, since we do not have access to the source code of Excel.

Furthermore, many of the optimizations in algorithms for simplifying mutual recursion---introduced by circular dependencies---overlook the fact that every function can be used multiple times. As such, there should be an efficient way to store the data computed from these functions.

Lastly, validating the semantics is challenging due to the limited availability of relevant datasets (as discussed in @subsec:insufficient-data). We can dynamically compute some calculations using the EPPlus library @epplus_software_epplus_nodate, but it does not support circular references. The Syncfusion library @syncfusion_syncfusion_nodate does support circular references, but it calculates them in a different way than Excel. Therefore, the calculation of synthetic data needs to be investigated.

== Expected Input from the Literature Survey

We expect the literature survey to have limited input on the thesis. The spreadsheet book by #citeauthor(<sestoft_spreadsheet_2006>) provides a foundation of the inner-workings of Spreadsheet programs, and the work of #citeauthor(<bock_semantics_2020>) offers a starting point for formalizing semantics, though further verification for Excel is required. The master theses discussing support graphs @iversen_runtime_2006 @poulsen_optimized_2007 will help the development of the first mapping to an internal domain model.
//Research on program generation will guide the translation of Excel functionality into high-level code.

== Experimental Design and Proof of Concept

The research will compute mappings between Excel functions and high-level code in the C\#. This includes designing a domain model that handles cyclic dependencies and dynamic spreadsheet behavior using iterative calculation techniques. We will develop a prototype to demonstrate the feasibility of the approach, focusing on accurately converting selected Excel formulas into human-readable C\# code. Once the initial domain model is constructed, testing will evaluate performance, correctness, and capabilities.

== Research Method and Validation
Since there is not much research on the area of mapping excel formulas to code, this thesis describes an exploratory study with design elements.

A mapping will be designed that will map Excel functions to executable, readable C\# code. Within this mapping, we will design a model that works with circular references and dynamic functions in Excel.

A quantitative approach will be used to evaluate the proof of concept, focusing on metrics such as execution time in seconds, memory usage in MiB, and transformation success rates. Empirical validation will be used to test the model against real-world datasets, comparing outputs to Excel, and validating edge cases in complex formulas.

== Hypotheses
Below, we explain our hypotheses on the research questions outlined in @subsec:research-questions. 
// === Does there exist a human-readable high-level code representation of complex compositions of excel formulas?
// We hypothesize there exists a human-readable representation of most formulas. However, we do believe there are certain restrictions on the compilation, especially when it comes to the complex dynamic formulas.

// ==== Does there exist a mapping for Dynamic Excel formulas?
// Our hypothesis is that a mapping for dynamic formulas will exist, but with restrictions. In highly complex scenarios, where multiple dynamic formulas are being used, compilation to normal imperative code may be difficult due to their use of the spreadsheet datastructure (something we are trying to remove).

// ==== Does there exist a mapping for cyclic references?


// ==== What are the differences between the Excel Formulas and the compiled code?

// === How can the mapping between excel formulas and code be verified?
// We theorize the mapping can be verified using two ways: empirical and formal. For empirical verification, we hyptohesise it can be done, but will require a lot of test cases to cover the complexity and edge-cases of excel formulas.

// For formal verification, we expect that we can model most of excel formulas. There will be some difficulty defining the formal semantics for the more complex formulas. We believe not the full semantics of the dynamic formulas can be mapped, due to their inherent complexities and their reliance on the spreadsheet datastructure.


#enum(numbering: n => strong[(RQ#n)], indent: -35pt, spacing: 12pt,
  [*How can complex compositions of excel formulas be mapped to human readable C\# code?*

  Our hypothesis is that by mapping the excel formula calculations to an internal domain model inspired by the support graph @poulsen_optimized_2007, we can map all of the 'static' excel formulas like `SUM` and numeric operations, also with complex operations. We expect a difficulty in readability when the formulas become quite complex and large, since there will probably be a lot of duplicate code, reducing the readability. We expect it to be quite difficult to reduce this duplication, since the dependencies will vary in every cell.#parbreak()
  ] + enum(numbering: n => strong[(RQ1.#n)], spacing: 12pt,
    [*How can cyclic dependencies with iterative calculations be mapped to C\# code?*

    Cyclic dependencies are being calculated through a fixed point algorithm in Excel. A straightforward solution is storing all these calculations in some sort of spreadsheet-like storage and iterating over them to calculate their fixed point. However, this is not clean, and can be optimized, we hypothesize. We think we can analyze the dependencies in the tree, as well as cells dependent on cells in the cyclic dependency, and generate an efficient model that stores as little intermediary calculations as possible.#parbreak()
    ],
    [*What are the performance differences between the Excel Formulas and different versions of compiled code?*
  
    We assume Excel is heavily optimized, since the tool has been around for a long time. Therefore, we hypothesize that Excel may be faster in small examples, albeit at the cost of memory. Excel must store the entire worksheet in memory @sestoft_spreadsheet_2006, whereas our tool will be more memory-efficient because we do not need to store every intermediate calculation separately and can reuse memory. As worksheets grow larger,we expect our compiled code to be faster than Excel because Excel must store all intermediate calculations---which is time-consuming---and our tool can perform optimizations in the execution path. However, our work will not support multi-threading, potentially resulting in poorer performance than Excel due to this missing feature.
    ]
  ),
  [*How can the mapping between excel formulas and code be verified?*

  We theorize the mapping can be verified using two ways: empirical and formal. For empirical verification, we hypothesize it can be done, but will require a lot of test cases to cover the complexity and edge-cases of excel formulas. We think this can be done by using an approach inspired by @fisher_automated_2002, where testing strategies are used in spreadsheet. Instead of using real-world data, which may be hard to come by (see @subsec:insufficient-data), synthetic data can be used to generate test cases using professional packages.

  For formal verification, we expect that we can model most of excel formulas. There will be some difficulty defining the formal semantics for the more complex formulas. We believe not the full semantics of the dynamic formulas can be mapped, due to their inherent complexities and their reliance on the spreadsheet data-structure.
  #parbreak()
  ],
)<researchQuestions:subQuestions>

== Timeline

The research will begin by defining mappings and researching efficient storage mechanisms for function results. Iterative calculation techniques will be developed and integrated into the domain model. Testing will then evaluate performance and correctness. The final phase will involve comprehensive testing, documentation, and demonstration of the prototype. Please see @sec:project-plan for the full timeline.

While we do not think it is likely to do more than presented in the previous paragraph, our stretch goal is to formally validate the conversion to C\# code by using the operational semantics.