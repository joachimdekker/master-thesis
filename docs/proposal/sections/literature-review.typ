/**
 * Cites the author
**/
#let citeauthor(ref) = cite(ref, form: "prose", style: "../ieee-short.csl")

= Related Work <related-work>
Existing work already helps in answering the questions we set out to answer. 

== Function Extraction
#citeauthor(<lano_agile_2017>) describe a way to convert an excel application to code by extracting a UML diagram out of a spreadsheet and converting it to code, but the whole process is manual. // Do we really want to make note of this?

Object oriented models are often used to validate spreadsheets to reduce errors @engels_classsheets_2005, @cunha_spreadsheet_2015. The automated extraction of such a model is also proposed by #citeauthor(<cunha_automatically_2010>), where functional dependencies are used to detect dependencies between columns like in database normalization, and construct a model of this. They augment this with OCL to also describe the models constraints @cunha_relational_2012.

#citeauthor(<sestoft_spreadsheet_2006>) describes a full alternative implementation of spreadsheets, with subsequent master theses expanding upon the work @iversen_runtime_2006 @poulsen_optimized_2007
describe how to efficiently calculate the values in a spreadsheet using the support graph: a more organized version of the functional dependencies used in @cunha_automatically_2010.

Outside the literature, few packages exist that support calling excel calculations. EPPlus @epplus_software_epplus_nodate, Espose Cells @aspose_espose_nodate, Apache POI @apache_software_apache_nodate, and SyncFusion @syncfusion_syncfusion_nodate all use their own calculation engine based on the dependencies, but do not use a support graph like in @sestoft_spreadsheet_2006. The latter also support circular dependencies but uses a different update model than in excel, where the early stopping (i.e. the definition of when the values do not differ that much) of the iterative update cannot be defined, it just updates the sheet a set amount of times.

== Validation
To verify the semantics of the program, the semantics of Excel should be defined so they can be compared with the semantics of the higher level language.

#citeauthor(<bock_semantics_2020>) defines the operational semantics for a self-made spreadsheet framework @sestoft_sheet-defined_2013 which closely reflects and closely resembles the Excel semantics. The semantics could serve as a starting point for verifying the compiler steps from Excel formulas to generated code, but need alteration because it is not clear if they fully model the excel semantics.

#citeauthor(<steinhofel_modular_2018>) describes validation of source-to-source compilation using symbolic execution, where the program is 'executed' using inference rules to create a symbolic execution tree that can be used to compare the two sources.
// Some more here?

Apart from formal validation, 
#citeauthor(<rothermel_methodology_2001>) and #citeauthor(<fisher_automated_2002>) introduce the _What You See Is What You Test_ (WYSIWYT) framework for spreadsheet testing which sets it apart in the emperical verification It utilizes definition-use (du) associations to link formulas to their computational or predicate uses. A spreadsheet is considered 'validated' when all du-associations are exercised by at least one test. While users can manually validate du-associations @rothermel_methodology_2001, the framework also supports generating automated test cases using random or goal-oriented approaches to satisfy all du-associations @fisher_automated_2002. For a good compilation, the generated code should pass all of these automated tests to be sufficiently verified.

// Translation Validation

== Source-to-source compilation
Source to source compilation, also called transpilation, transcompilation @roziere_unsupervised_2020, or program extraction, involves the translation of one higher-level source language to a target higher-level language. The compilation should, like a normal compiler, preserve the semantics. Several techniques have been implemented.

#citeauthor(<waters_program_1988>) and #citeauthor(<ordonez_camacho_automated_2010>) both describe _transliteration and refinement_, as well as _abstraction and reimplementation_ @waters_program_1988. Transliteration involves the translation of code line-by-line to a intermediate translated representation in the target language @waters_program_1988. The intermediate representation can then be refined to better source code @waters_program_1988. @ordonez_camacho_automated_2010 translate grammars through transliteration, converting one language to the other using simple rules. They allow the user to define exceptions, which serve the same role, albeit a bit more expressive than refinement. #citeauthor(<cockx_reasonable_2022>) use simple rules to translate Agda programs into Haskell programs, essentially only doing the transliteration step.

_Abstraction and reimplementation_ is a more used method @waters_program_1988 @ordonez_camacho_automated_2010 @lopes_chomsky_2005. From the source language, an intermediary representation is extracted, such as ILR @ordonez_camacho_automated_2010 or UML+OCL @lano_using_2024. The intermediary representation is then converted to the target language @waters_program_1988. This method only needs $2n$ instead of $n(n-1)$ translators @ordonez_camacho_automated_2010 @lopes_chomsky_2005.

More recently, training-based models have gained traction @yang_exploring_2024 @roziere_unsupervised_2020. #citeauthor(<roziere_unsupervised_2020>) 

More recently, Large Language Models (LLMs) have become a dominant approach for source-to-source compilation @yang_exploring_2024 @roziere_unsupervised_2020 @chen_tree--tree_2018. These methods leverage the ability of LLMs to generalize across tasks without specific re-training and provide guidance in their shortcomings @yang_exploring_2024. #citeauthor(<yang_exploring_2024>) propose using test case generation to provide hints to repair incorrect translations. 

// PROOFREAD -> Unsupervised Machine Translation (UMT) techniques train models using monolingual data with denoising auto-encoding and back-translation. Other methods include unit test-based improvements (TransCoder-ST), graph-based models (GraphCodeBERT), and rule-based transpilers (C2Rust, CxGo), which rely on manually crafted translation rules but are labor-intensive and language-dependent.

TBD...

== Mutual Exclusion


// A definition-use (du) association in spreadsheets links a cell's defining formula with its computational or predicate uses. Under the du-adequacy criterion, testing is sufficient when every du-association is exercised by at least one test, validated as "correct" by user verification.

// Discuss the literature related to your proposal. Focu s on concepts and ideas, how this relates to your proposal. Do not describe each paper individually, but as a collective.

// Aim for a "matrix" structure of the related work: What are the features desired from the outcome of your research? How are these features delivered by existing work? Are there gaps unanswered by existing work?

// You can use BibLaTeX to keep track of your references in a separate file (`references.bib` in this template). To cite a paper, use `\cite{key}`, for example `\cite{ISO25010}`, becomes @ISO25010. You can also directly cite the author by using `\citeauthor{key}` and `\citetitle{key}` for the title. For example:

// #quote(block: true)[
// `As \citeauthor{Beck1999} shows in \citetitle{Beck1999}~\cite{Beck1999}`
// ]

// becomes

// #quote(block: true)[
//   As #cite(<Beck1999>, form: "prose") shows in ~@Beck1999
// ]

// A guideline is to include between 10 and 20 papers on your topic in the related work. The exact number depends on the topic and available literature.


// Kleene Fix Point Theorem

// Recursion Theory

// Tail recursion

