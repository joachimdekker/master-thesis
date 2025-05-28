= Type Inference Algorithm
Just like Python, Excel is dynamically typed. The type of the cells is inferred ar runtime. If we want to convert Excel computations to a 

= When do we inference types
We want to inference types as early in the process as possible.

== Type inference
When inferencing types, we use inference rules. When iterating over the Compute Graph, we look at every node and use an inductive approach. First, we determine the type of the leaves and then determine the type of the parent based on the inference rules.