= Introduction <sec:introduction>
Microsoft Excel is the biggest programming language around the world @gordon_lambda_2021, used all around the globe. At pension fund companies, it is used to maintain and calculate the pension fund interest for all of its customers. Pension fund employees are using Excel worksheets as a 'sandbox' to produce optimal forecasts under a range of different situations. These situations differ per person, and are being represented by a wide array of parameters. Each year, a third-party actuarial calculation tool---a variant of excel---is used to calculate the forecasts for more than one million pension fund users. 

According to one of the partners of the pension funds, Info Support B.V., this tool is slow, and requires manual conversion of the calculations in the excel file into the tool. Hence, Info Support---as part of the ongoing _GROENpensioen_ project---aims to convert these calculations from Microsoft Excel to high performing code, completely skipping the third-party tool. While there have been work on extracting models from spreadsheets (see @sec:related-work), there is no tool to convert a spreadsheet to code. 

This process entails multiple steps, from compiling excel formulas to a generic programming language to applying optimalizations to the dataflow. In this thesis, we will explore the former problem of compiling excel files to coherent, high-performing higher level programming language code in the context of actuarial computations. This approach will therefore focus on the computational model of the excel engine.

== Description of the problem <subsec:problem-description>
This is not a straightforward problem to solve. First of all, the excel calculations can become quite complex when using large compositions of formulae---which in turn can have complex semantics---and cells. Furthermore, pension fund employees also make use of more advanced Excel features such as the fixed step iteration tool, which will activate when there are circular references in the file. Consequently, the compiled code should also support fixed step iteration.

Additionally, the compiled code needs to be semantically the same as the excel calculations. The code needs to be verified in a way to ensure the employee the compiled code is exactly the same as his excel worksheet.

Furthermore, human auditors must inspect the program. Besides it needing to be semantically equivalent, it needs to be readable, as the auditor should be able to easily read and understand the code, not decipher it because it is in bytecode. This also means that we cannot rely on the matrix spreadsheet structure, since that would be really hard to read in code. The translation step will thus be a challenging aspect of this thesis.

== Research Questions<subsec:research-questions>
On the basis of the description of the problem and challenges, we propose the following research questions.

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [How can complex compositions of excel formulas be mapped to human readable .NET code?<RQ1>] + enum(numbering: n => strong[(RQ1.#n)], indent: -15pt,
    [How can cyclic dependencies with iterative calculations be mapped to .NET code?<RQ1.1>],
    [What are the performance differences between the Excel Formulas and the compiled code?<RQ1.2>]
  ),
  [How can the mapping between excel formulas and code be verified?<RQ2>],
)<researchQuestions:subQuestions>

We have already explained the relevance of #link(<RQ1>, [*(RQ1)*]), *(RQ1.1)*, and *(RQ2)* in @subsec:problem-description, we will now only elaborate on *(RQ1.2)*. Since the previous tool was quite slow, and this research has the potential to be used in calculations of over 1 million users, we want to measure if using the compilation tool is faster than the Excel calculations.

== Disadvantages of existing tools

We acknowledge there are existing tools to calculate the result of an excel formula. However, they do not transform the program to a readable piece of code in a higher level programming language. Furthermore, they often do not support iterative calculation where cells in a cyclic reference are bein updated using fixed point iteration. 

In contrast with existing research on extracting models from spreadsheets into relational databases @cunha_spreadsheets_2009, where the whole spreadsheet is and all of its data is extracted into a model, focussing on the computational model provides a unique viewpoint where we only consider the output of selected cells and see the excel sheet as an 'input-output' model.