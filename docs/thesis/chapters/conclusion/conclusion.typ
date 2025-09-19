= Conclusion

In this thesis, we set out to create an Excel compiler that produces clear, human-readable C\# code using experimental compiler design. We introduce _structure-aware compilation_ that uses the structure of the Excel file as heuristic for more readable code. We found two structures commonly used in Excel files and implemented a three-phase compiler called Excelerate that utilises this technique using a separate IR for each stage.

This study was based on three research questions:

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [How can complex Excel formula compositions be mapped to human-readable C\# code?],
  [What are common excel structures and how can they be applied to optimise the compilation of excel formula compositions?],
  [What are the performance differences between Excelerate (compiled code) and Excel?],
  [How can the mapping between excel formulas and code be verified?]
)

== Mapping Excel formulae to human-readable C\# code
We employed a multi-phase compiler to transform the complex Excel formula compositions present in the _Worksheet_ to human-readable C\# code. This multi-phase compiler was built to be modular, and separates the different phases that we had to use. The _Structure Phase_ constructs the _Structure Model_: A rich AST of the Excel file, supplemented with _Structures_ found within the _Spreadsheets_. 

This AST is used in the _Compute Phase_ to create the _Compute Grid_, the first part of the _Compute Model_. It allows the compiler to insert the found structures and adjust references to these structures in other nodes. These nodes are linked to form the _Compute Graph_: a representation of the Excel computational model. 

The _Compute Graph_ is then transformed to the _Code Model_ in the _Code Phase_ where we apply final transformations to make the code human readable. Finally, we emit the _Code Phase_ to C\# using the Roslyn Compiler.

== Common Excel Structures

We identified two structures based on the Microsoft Create Excel Template Repository: the _Table_ and the _Chain_. The _Table_ models a conventional rectangular worksheet region. The _Chain_ is a table with rows that depend on previous rows. 

During the _Structure Phase_, we detect these structures and use their information to inject semantics about the domain of the spreadsheet into the generated C\# code. Furthermore, the detection of these structures eliminates repetitive calculations---especially in chain structures. Excelerate uses separate classes to remove this code duplication from the final generation. As a result, the compiler optimises the code for readability and performance.

== Verification

We employed randomized differential testing against Excels native calculation engine in order to verify the semantic preservation between Excel and Excelerate. The results in @chapter-evaluation show strong evidence for semantic equality, suggesting the mapping is successful for the supported subset of the Excel functions and operators. Furthermore, a qualitative analysis was done on the readability of the code, resulting in improvements from 'basic' compilation without _structure-aware compilation_, providing evidence that the code is human-readable while concluding that improvements can still be made.

== Performance differences

The evaluation in @chapter-evaluation answered the final research question by conducting an experiment comparing Excel with compiled Excelerate code using randomised inputs. There were no discrepancies found in semantic equality. We observed a linear trend between spreadsheet complexity and calculation time. Furthermore, Excelerate was significantly faster, achieving an average speedup of $677 plus.minus 90$x. The omission of the COM interface played a big role in this. The significant speedup over Excel allows Excelerate to substitute in business applications where performance is critical.

Due to scope limits, only a small subset of Excels functions and features was mapped. This limits generalisability of the results to all spreadsheets. Furthermore, due to the COM interface overhead that we have to use during testing, the results for speedup may be skewed if this interface is not used for communicating with Excel.

Ultimately, this thesis demonstrates that Excels underlying computational model can be compiled into clean, idiomatic C\# code with verified semantics and significant performance gains. For companies that are extensively using excel, this can open the door to faster and safer execution.


// // Based on spreadsheets in the Microsoft Templates directory, we identified two structures that provided heuristics for compiling more readable code. This _structure-aware compilation_ used a three-phase compiler design. 

// - In this thesis, we set out to investigate the possibility of compiling Excel to clear, human-readable C\# code. 

// - RQ1: Multi-phase compiler, three steps with Structural, Compute and Code Layout, identifying structures with structural-compilation: letting the structures guide the compilation.
// - RQ2: we identified tables and chains, and exploited these patterns for a significant improvement in readability.
// - RQ3: we demonstrated that the generated Csharp code consistently outperforms Excel across all benchmark workbooks.
// - RQ4: we introduced a method that generates 'unit tests' for the compiler by parametrising all available input and generating random input to test conformance to Excel's calculation engine.

// - Structure-aware compilation can bridge the gap between Excel and readable code.

// - Conclude and answer the research questions

// - Sum up the contributions of the thesis once more
//  - Structural Compilation
//  - Excel IR
//  - Evaluation Techniques?