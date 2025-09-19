= Outline

After the background on the literature and Excel, we can start discussing the Excel compilers we introduce in this thesis. The remainder of this thesis is organised as follows.

In @chapter-compiling-excel, we introduce a 'basic' Excel compiler and explain the three phases (Structural, Compute, and Code) that transform an Excel workbook into a C\# library. We show that the compiler produces correct code, but discuss limitations in readability.

Hoping to improve this readability, we introduce a more advanced compiler Excelerate and the concept of _Structure-aware Compilation_ in @chapter-excelerate. The compiler builds upon the 'basic' compiler. This chapter details the _Table_ and _Chain_ structures and covers how they are integrated into the 'basic' compiler.

@chapter-evaluation describes the methodology and results for our experiments that evaluate Excelerate and Excel using differential verification. We assess semantic equality, benchmark performance and evaluate the readability of the emitted code. 

Finally, @chapter-conclusion concludes the thesis, summarising the main findings and contributions, answering the research questions and outlines potential improvements for further work.