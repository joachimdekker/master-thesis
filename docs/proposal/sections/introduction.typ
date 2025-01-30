= Introduction <sec:introduction>
Because of its widespread use and ease-of-use, Microsoft Excel is being used in pension fund companies to maintain and calculate the pension fund for all of its customers. Info Support, as part of the ongoing _GROENpensioen_ project aims to convert these calculations from Microsoft Excel to high performing code.

Pension funds operators use Excel as a 'sandbox' to test model parameters for optimal forecasts. These parameters vary widely and often differ per person to calculate the pension for. Each year, the third-party actuarial calculation tool (which could be seen as a variant of Excel) calculates forecasts for all pension holders. However, changes made by pension fund operators must be manually translated into the tool. The tool itself is 'very slow' according to Info Support and could be improved. Ideally, they want a faster system that can allow users to quickly visualize projected pension outcomes.

Consequently, they want to adopt the 'sandbox' environment and develop a tool capable of optimizing and transforming these Excel-based models to high-performing code. This process entails multiple steps, from compiling excel formulas to a generic programming language to applying optimalizations to the dataflow. In this thesis, we will explore the former problem of compiling excel files to coherent, high-performing higher level programming language code in the context of actuarial computations. This approach allows us to focus on the computational model of the excel engine.

The actuarial calculations need to be inspected by auditors, so the mapping needs to be verified and do exactly as the operator wants it to be. And it needs to be readable, as the auditor should be able to read the code, not decipher it because it is in bytecode.

While there are existing calculation 'engines', they do not transform the program to a readable piece of code in a higher level programming language. Furthermore, they often do not support iterative calculation where cells in a cyclic reference are bein updated using fixed point iteration. 

In contrast with existing research on extracting models from spreadsheets into relational databases @cunha_spreadsheets_2009, where the whole spreadsheet is and all of its data is extracted into a model, focussing on the computational model provides a unique viewpoint where we only consider the output of selected cells and see the excel sheet as an 'input-output' model.

Based on the above, the thesis seeks to answer the following questions.

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [How can complex compositions of excel formulas be mapped to human readable .NET code?] + enum(numbering: n => strong[(RQ1.#n)], indent: -15pt, 
    [CHOOSE: How can Dynamic Excel formulas like `TRANSPOSE`, `IF`, and `INDEX` be represented in a support graph?],
    [CHOOSE: How can cyclic dependencies with iterative calculations be mapped to .NET code?],
    [What are the differences between the Excel Formulas and the compiled code?]
  ),
  [How can the mapping between excel formulas and code be verified?],
)<researchQuestions:subQuestions>