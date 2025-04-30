#import "../../utils/chapters.typ": chapter

#chapter("Introduction")

Microsoft Excel is arguably the most widely used programming environment worldwide @gordon_lambda_2021. The interface can be seen as the IDE, and the worksheet and the formulas as the code. At pension fund companies, it is used to maintain and calculate the pension fund interest for all of its customers. 

Pension fund employees use Excel worksheets as their tool to compute the forecasts under a range of different situations. There are more than a thousand situations per person. For example, a user might want to know what their pension will look like if they take their pension when 65 years old, when the economy has been good. All of these situations need to be calculated for nearly 1 million customers every year.

One of the partners of the pension funds, Info Support B.V., converted the tool to a high-level programming language, but this was done manually. Since the pension fund employees are not able to program and extend the code, when another scenario should be added, only the excel file is updated and needs to be manually converted by info support. Hence, Info Support---as part of the ongoing _GROENpensioen_ project---aims to automatically convert these calculations from Microsoft Excel to high performing code to improve reliability, performance and durability. While there have been work on extracting models from spreadsheets, there is no tool to convert the calculations in a spreadsheet to higher-level code.

The converting process entails multiple steps, from compiling excel formulas to a generic programming language to applying optimalizations to the dataflow. In this thesis, we will explore the former problem of compiling excel files to coherent, high-performing higher level programming language code in the context of actuarial computations. This approach will therefore focus on the computational model of the excel engine.

// Also say something about editing the scenarios in Excel, and that they thus need to do something about it.

= Problem Description <subsec:problem-description>
This problem is not straightforward to solve. First of all, excel calculations can become quite complex when using large compositions of formulae---which in turn can have complex semantics---and using large amounts of cells. Furthermore, pension fund employees also make use of more advanced Excel features such as the fixed step iteration tool, which will activate when there are circular references in the file. Consequently, the compiled code should also support fixed step iteration. Besides, the tool has to support over 300 different scenarios under three or more forecasting scenarios (good, bad, or normal economy), which means there are a lot of different execution paths the code needs to consider and the tool needs to support.

Additionally, the compiled code needs to be semantically the same as the excel calculations. The code needs to be verified in a way to ensure the employee the compiled code is exactly the same as his excel worksheet.

Furthermore, human auditors must inspect the program. Besides it needing to be semantically equivalent, it needs to be readable, as the auditor should be able to easily read and understand the code, not decipher it because it is in bytecode. Given that a normal 'matrix spreadsheet structure' would be hard to read, we have to find a way to convert this structure to high-level code. The translation step will thus be a challenging aspect of this thesis.

== Research Questions<subsec:research-questions>
On the basis of the description of the problem and challenges, we propose the following research questions.

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [How can complex Excel formula compositions be mapped to human-readable C\# code?<RQ1>] + enum(numbering: n => strong[(RQ1.#n)], indent: -15pt,
    [How can cyclic dependencies with iterative calculations be mapped to C\# code?<RQ1.1>],
    [What are the performance differences between the Excel Formulas and the compiled code?<RQ1.2>]
  ),
  [How can the mapping between excel formulas and code be verified?<RQ2>],
)<researchQuestions:subQuestions>

The relevance of #link(<RQ1>, [*(RQ1)*]), *(RQ1.1)*, and *(RQ2)* was discussed in @subsec:problem-description; we will now elaborate on *(RQ1.2)*. Since the previous tool was quite slow, and this research has the potential to be used in calculations of over 1 million users, we want to measure if using the compilation tool is faster than the Excel calculations.

#include "literature-research.typ"