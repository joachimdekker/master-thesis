== Research Questions
On the basis of the description of the problem and challenges, we propose the following research questions.

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [How can complex Excel formula compositions be mapped to human-readable C\# code?<RQ1>] + enum(numbering: n => strong[(RQ1.#n)], indent: -15pt,
    [How can cyclic dependencies with iterative calculations be mapped to C\# code?<RQ1.1>],
    [What are the performance differences between the Excel Formulas and the compiled code?<RQ1.2>]
  ),
  [How can the mapping between excel formulas and code be verified?<RQ2>],
)<researchQuestions:subQuestions>

The relevance of #link(<RQ1>, [*(RQ1)*]), *(RQ1.1)*, and *(RQ2)* was discussed in @sec:problem-description; we will now elaborate on *(RQ1.2)*. Since the previous tool was quite slow, and this research has the potential to be used in calculations of over 1 million users, we want to measure if using the compilation tool is faster than the Excel calculations.