#text([Abstract], weight: "bold", size: 2em)

Microsoft Excel is the most widely used end-user programming platform, yet its formula calculation engine presents a challenge for large scale calculations in the form of long delays and unreliability. This thesis presents _Excelerate_, a novel source-to-source compiler that compiles Excel files to idiomatic C\# libraries while preserving complete computational semantics. It was developed in collaboration with Info Support B.V. as part of the GROENpensioen project, which aims to modernize and accelerate large-scale pension-fund calculations.

The compiler employs a three-phase pipeline: a _Structural Model_ parses the Excel Workbook to extract spreadsheets and tables; a _Compute Model_ derives the dependency graph of formula compositions; and a _Code Model_ emits imperative C\# through the use of the Roslyn API. Building on this approach, this thesis introduces _structure-aware compilation_ that automatically detects higher-level spreadsheet structures, such as tables, to produce concise, and maintainable code.

Evaluation is done on five real-world Microsoft Excel templates and demonstrates semantic equality with Excels native calculation engine. Excelerate exhibits a speed-up of 1710x over Excel, indicating substantial performance improvements. These findings show that complex Excel formula compositions within a workbook can be transformed into idiomatic C\# code.

#pagebreak(weak: true)