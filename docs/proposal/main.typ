#set heading(numbering: "1.")

#set page(numbering: "1", )


#let citeauthor(ref) = cite(ref, form: "prose", style: "ieee-short.csl")

= Project Details <sec:projectDetails>

#table(
  columns: 2,
  stroke: none,
  align: (left, left),
  [*Project Title*], [Compiling Complex Actuarial Excel Computations into High-Performance Code.],
  [*Student*], [Joachim Dekker, 15887715],
  [*Academic Supervisors*], [dr. Andrés Goens Jokisch (PCS), a.goens\@uva.nl],
  [*Host*], [#link("https://www.infosupport.com/", [Info Support B.V.]), Business Unit Finance (BUF) at #link("https://maps.app.goo.gl/8PizXQEhjCxAAm7PA", [Kruisboog 42, 3905TG Veenendaal])],
  [*Contact Persons*], [Daily supervisor: Bjorn Jacobs, Software Developer, Info Support B.V., Bjorn.Jacobs\@infosupport.com, #linebreak() Graduation-coordinators, Info Support B.V., afstudeercoordinatoren\@infosupport.com ],
  [*Start Date*], [February 21st, 2025 (1 day per week), April 2025 (full-time)],
  [*Planned End Date*], [June 27th, 2025],
)


#include "sections/introduction.typ"
#include "sections/literature-review.typ"
#include "sections/methodology.typ"
#include "sections/risks.typ"
#include "sections/ethics.typ"
#include "sections/planning.typ"

#pagebreak()
#bibliography("references.bib", full: false, style: "ieee")