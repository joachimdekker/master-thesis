#import "../../utils/chapters.typ": chapter

#chapter("Compiling Excel")

#include "compiler-structure.typ"

#include "ahead-of-time-compilation.typ"

#include "structure-aware-compilation.typ"

= High Level Overview
Before we dive deeper into the intermediate representations, it is helpful to discuss the high level overview of the compiler. This section, we walk through the biggest steps of the compiler and relate the different steps and structural models to each other. In subsequent sections, we dive deeper in the actual structure of all the models. 

#include "examples/budget-example.typ"
#include "examples/budget-income.typ"

#include "overview.typ"

#include "intermediate-representations/structural.typ"
#include "intermediate-representations/compute.typ"
#include "intermediate-representations/data.typ"
#include "intermediate-representations/code-layout.typ"