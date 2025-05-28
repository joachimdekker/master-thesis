= Compiler Structure
Just like any other compilers, the Excel compiler consists of tiny steps that all contribute towards the common goal: readable C\# code. In between these steps, the code is represented using different languages. The languages or _intermediate representations_ (IR) help abstract the details of the Excel code and structure.

== One or many IRs
Within compiler construction, we can take as many steps as we want. Sometimes, it suffices to directly compile from one source to the other source, such as in simple DSLs. That said, it is often easier to create an intermediate representation so transformations on the AST are much easier. But how many representations are the best fit? In the following subsections, we discuss the advantages and disadvantages of having many intermediate representations.

=== One IR

Using a single IR simplifies the architecture and makes the compiler pipeline easier to reason about. With one representation, all transformation steps happen in a unified format, and the state of the compilation is centralized.

However, a monolithic IR must serve too many purposes—capturing structure, logic, and target code layout simultaneously—which often leads to a bloated and inconsistent format. It becomes harder to maintain and more challenging to distinguish between high-level intent and low-level mechanics.

=== Many IRs

In contrast, using multiple IRs introduces overhead and boilerplate, but allows for separation of concerns. Here, each IR can focus on one specific aspect of the compilation process, such as the logical structure, data flow, and code generation. This focus makes it easier to reason and permits modular, small transformations generating readable code. 

Having many IRs also mean more transformation steps. Furthermore, on the one hand, it makes debugging harder since many information has to be tracked across the models. That said, we can apply assertion at the end of each 'layer' to make sure the model is up to standard before transforming it to the next model.

=== Excel Compiler
For the Excel compiler, we use multiple intermediate representations but rely on references between them to keep them interconnected. In the next sections, we introduce the four different IRs—Structure, Compute, Data, and Code Layout—and explore the rationale behind their existence and their internal design.