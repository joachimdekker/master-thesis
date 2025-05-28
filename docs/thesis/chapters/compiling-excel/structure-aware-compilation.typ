== Structure-aware compilation

Unlike traditional compilation, where trivia like whitespace and comments are often discarded [source?], this compiler heavily relies on the structure of the spreadsheet. The spreadsheet contains a lot of data that can be used in the compilation, be it in the form of how cells are populated: table-like or randomly. Spreadsheets contain common patterns that can be abstracted, such as tables with- or without interdependent rows.

In traditional compilation, the result of the compilation is often used for byte code or code that a human does not read. This means that whitespace and comments can just be removed, as they often do not contain usable information. Furthermore, trivia in the AST can be difficult to work with, and hence it is trivially removed [source?]. 

In this thesis, we introduce _structure-aware compilation_, where the compiler uses the structure of the source language as context in the compilation passes in order to improve the legibility of the code. Semantically similar spreadsheets that are different in layout will therefore likely result in different outputs. 
[Insert example?]
Within _structure-aware compilation_, we always maintain an abstracted representation of the structure of the source language. This abstracted representation is meant for use in all subsequent passes. Compilation of a source language will thus always begin with the extraction of the abstracted representation of the structure.

In the context of Excel, the structure-aware compilation introduces the _Structural Model_, which partially maps the Excel Workbook. It is important to note that this structure-aware compilation is different from the underlying compute model, which is being modelled by a different intermediate representation.