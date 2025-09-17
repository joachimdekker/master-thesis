== Research Questions
To address these problems, we utilise an exploratory study with design elements. Within the study, we propose the following research questions to guide the design process:

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [How can complex Excel formula compositions be mapped to human-readable C\# code?<research-questions:main> ],
  [What are common excel structures and how can they be applied to optimise the compilation of excel formula compositions?<research-questions:structures> ],
  [What are the performance differences between Excelerate and Excel?<research-questions:performance> ],
  [How can the mapping between excel formulas and code be verified?<research-questions:mapping-verfication> ],
)<research-questions>

The relevance of these research questions have been made clear in the previous sections. The main research question (RQ1) is the main goal of the compiler design, since we need to find a way to convert the Excel files to code in a idiomatic way. The second research question (RQ2) helps with this, providing structures and seeing if they can help guide the compiler. The last two research questions are the evaluation of the compiler, searching for performance improvements and semantic equality.