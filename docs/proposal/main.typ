#import "@preview/timeliney:0.1.0"

#set heading(numbering: "1.")

#let citeauthor(ref) = cite(ref, form: "prose", style: "ieee-short.csl")

= Project Details <sec:projectDetails>

#table(
  columns: 2,
  stroke: none,
  align: (left, left),
  [*Project Title*], [Compiling complex actuarial excel computations to high-performing code.],
  [*Student*], [Joachim Dekker, 15887715],
  [*Academic Supervisors*], [dr. Andrés Goens Jokisch (PCS), a.goens\@uva.nl],
  [*Host*], [#link("https://www.infosupport.com/", [Info Support B.V.]), Business Unit Finance (BUF) at #link("https://maps.app.goo.gl/8PizXQEhjCxAAm7PA", [Kruisboog 42, 3905TG Veenendaal])],
  [*Contact Persons*], [Daily supervisor: Bjorn Jacobs, Software Developer, Info Support B.V., Bjorn.Jacobs\@infosupport.com, #linebreak() Graduation-coordinators, Info Support B.V., afstudeercoordinatoren\@infosupport.com ],
  [*Start Date*], [February 7th 2025 (1 day per week), April 2025 (full-time)],
  [*Planned End Date*], [June 27th 2025],
)

#include "sections/introduction.typ"
#include "sections/literature-review.typ"
#include "sections/methodology.typ"
#include "sections/risks.typ"

= Ethics <ethics> // Add pension data possibility
There are no foreseeable ethical doubts or dilemmas on all areas. The project will not involve human research, data sets about people or cyber-security or online privacy issues. While we could use an LLM like Codex to make the code more 'human readable', this does not pose any threats. No sensitive or personal data is involved. Additionally, the outputs are fully explainable and align with the practices of experienced developers, mitigating any risks of misinterpretation or unintended consequences. As such, no ethical doubts or dilemmas are anticipated in any area of the project.

// Fill out the ethics self-check via the RMS portal for Faculty of Science#footnote[#link("https://rms.uva.nl/servicedesk/customer/portal/14");];, and present a summary of the result and future actions. The main areas for ethical concerns are:

// - Research that includes active involvement of human research participants and/or gathering of new data from human participants

// - Research that involves the inclusion, combination, use, and/or analysis of already existing data sets about people

// - Design or development of AI-technologies/algorithms, or deployment and/or use of AI-technologies/algorithms for practical applications

// - Research that involves cyber security and/or online privacy issues

// If the self-check does not lead to an adequate answer to doubts and dilemmas, consult the Ethical Review Committee (ECIS-SP for SE students) via the Ethics portal.

= Project Plan <project-plan>
See the indefinite project plan below. We propose to write the thesis alongside the research, since the research will be incremental.

#figure(
  timeliney.timeline(
  show-grid: false,
  {
    import timeliney: *
    
    let dt(month, day) = datetime(year: 2025, month: month, day: day)
    
    let startDate = datetime(year: 2025, month: 2, day: 7)
    let endDate = dt(7,20)
    
    let calcTime(start, end) = {
      return int((end - start).days()) + 1
    }

    let at(month, day) = {
      return calcTime(startDate, dt(month, day))
    }

    let inWeek(weekNr) = {
      return weekNr * 7
    }

    let weeks(startWeek, endWeek) = {
      return (inWeek(startWeek - 1), inWeek(endWeek))
    }
    
    headerline(
      group(([*Feb*], calcTime(dt(2,3), dt(2,28)))),
      group(([*Mar*], calcTime(dt(3,1), dt(3,31)))),
      group(([*Apr*], calcTime(dt(4,1), dt(4,30)))),
      group(([*May*], calcTime(dt(5,1), dt(5,31)))),
      group(([*June*], calcTime(dt(6,1), dt(6,30)))),
      group(([*July*], calcTime(dt(7,1), endDate))),
    )
    
    headerline(
      group(([*Block 4*], 7 * 8)), 
      group(([*Block 5*], 7 * 9)), 
      group(([*Block 6*], 7 * 4)),
      group(([*Late*], calcTime(dt(6, 30), endDate)))
    )

    headerline(
      group(..range(8).map(n => ([#(n+1)], 7))),
      group(..range(9).map(n => ([#(n+9)], 7))),
      group(..range(4).map(n => ([#(n+18)], 7))),
      group(..range(3).map(n => ([#(n+22)], 7))),
    )
    
    taskgroup(title: [*Research*], {
      task([Domain Model \ for Simple Mappings], weeks(1, 4), style: (stroke: 2pt + gray))
      task([Code Generation], weeks(1, 4), style: (stroke: 2pt + gray))
      task([Domain Model \ for Cyclic References ], weeks(1, 4), style: (stroke: 2pt + gray))
      task([Domain Model \ for Dynamic Formulas], weeks(1, 4), style: (stroke: 2pt + gray))
    })

    taskgroup(title: [*Validation*], {
      task([Test Setup], weeks(4, 5), style: (stroke: 2pt + gray))
      task([Create test setup], weeks(4, 5), style: (stroke: 2pt + gray))
      task([Create test setup], weeks(4, 5), style: (stroke: 2pt + gray))
    })

    taskgroup(title: [*Thesis Writing*], {
      task([Draft], weeks(9, 14), style: (stroke: 2pt + gray))
      task([Release Candidate], weeks(15, 18), style: (stroke: 2pt + gray))
      task([Final Version], weeks(19, 21), style: (stroke: 2pt + gray))
    })

    milestone(
      at: at(3,4),
      style: (stroke: (dash: "dashed")),
      align(center, [
        *Conference demo*\
        Dec 2023
      ])
    )

    milestone(
      at: 6.5,
      style: (stroke: (dash: "dashed")),
      align(center, [
        *App store launch*\
        Aug 2024
      ])
    )
  }
), caption: [
  Gantt chart of the project plan.
],
supplement: "Timeline")
<fig:gantt-chart>

#pagebreak()
#bibliography("references.bib", full: false, style: "ieee")