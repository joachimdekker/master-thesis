#import "@preview/timeliney:0.1.0"

#let citeauthor(ref) = cite(ref, form: "prose", style: "ieee-short.csl")

= Project Details <sec:projectDetails>

#table(
  columns: 2,
  stroke: none,
  align: (left, left),
  [*Project Title*], [Compiling complex actuarial excel computations to high-performing code.],
  [*Student*], [Joachim Dekker, 15887715],
  [*Academic Supervisors*], [dr. Andres Goens Jokisch (IVI), a.goens\@uva.nl],
  [*Host*], [#link("https://www.infosupport.com/", [Info Support B.V.]), Business Unit Finance (BUF) at #link("https://maps.app.goo.gl/8PizXQEhjCxAAm7PA", [Kruisboog 42, 3905TG Veenendaal])],
  [*Contact Persons*], [Daily supervisor: Bjorn Jacobs, Software Developer, Info Support B.V., Bjorn.Jacobs\@infosupport.com, #linebreak() Graduation-coordinators, Info Support B.V., afstudeercoordinatoren\@infosupport.com ],
  [*Start Date*], [February 7th 2025 (1 day per week), April 2025 (full-time)],
  [*Planned End Date*], [June 27th 2025],
)

= Introduction <sec:introduction>
Because of its widespread use and ease-of-use, Microsoft Excel is being used in pension fund companies to maintain and calculate the pension fund for all of its customers. Info Support, as part of the ongoing _GROENpensioen_ project aims to convert these calculations from Microsoft Excel to high performing code.

Pension funds operators use Excel as a 'sandbox' to test model parameters for optimal forecasts. These parameters vary widely and often differ per person to calculate the pension for. Each year, the third-party actuarial calculation tool (which could be seen as a variant of Excel) calculates forecasts for all pension holders. However, changes made by pension fund operators must be manually translated into the tool. As already said, the tool itself is 'very slow' according to Rinse and could be improved. Ideally, they want a faster system that can allow users to quickly visualize projected pension outcomes.

Consequently, they want to adopt the 'sandbox' environment and develop a tool capable of optimizing and transforming these Excel-based models to high-performing code. This process entails multiple steps, from compiling excel formulas to a generic programming language to applying optimalizations to the dataflow. In this thesis, we will explore the former problem of compiling excel files to coherent, high-performing higher level programming language code in the context of actuarial computations. This approach allows us to focus on the computational model of the excel engine.

In contrast with existing research on extracting models from spreadsheets into relational databases @cunha_spreadsheets_2009, where the whole spreadsheet is and all of its data is extracted into a model, focussing on the computational model provides a unique viewpoint where we only consider the output of selected cells and see the excel sheet as an 'input-output' model. It begs the following question:

#block([*(RQ)* Does there exist a formal mapping from Excel sheets to high level code?], inset: 10pt)<researchQuestions:mainQuestion>

This question contains several other questions, namely

#enum(numbering: n => strong[(RQ#n)], indent: 10pt, 
  [Can a compiler between Excel sheets and high level code be constructed?],
  [What is the difference between the compiled program and excel formulas?], 
  [How can the mapping between excel formulas and code be verified?],
)<researchQuestions:subQuestions>


// Perhaps write some more things here :)

// People are used to Excel and have not programmed in another programming language. Many users have years of experience with Excel, but have never programmed in a programming language before. Therefore, it is easier to just stick to Excel instead of converting the whole organization to use another tool.


// Still, the following questions persist:

// + What are the steps you follow when converting an Excel sheet into executable code?
//   + What do you consider the most challenging step in this process? What makes this step so difficult?

//       // + pensione transitie -> software voor pensioenen: berekenen hoeveel pensioen je nu hebt opgebouwd en wat de prognose is
//       // Het nieuwe pensioenstelsel is nu een belgginsrekening
//       // Geld mag niet in een keer uitkeren
//       // Pensioen 'kopen'-> Voor zoalng als jij leeft betalen wij je x per maand.
//       // 1. Wat heb je nu opgebouwd
//       // 2. Prognoses -> wat ga je krijgen?
//       // 3. Wat voor type pensioen kan je dan kopen?
    
//       // Actuariele rekenprogrammas, waarin ze uitgemodeleerd worden. Berekeningen zijn ze niet heel moeilijk.
//       // Niet super complex -> Allemaal parameters waar je mee kan draaien.
//       // In excel uitgeprogrammeerd.
//       // In actuariele rekenpakketten, maar die zijn niet snel, een keer per maand worden die berekeningen gemaakt. 
//       // Berekeningen moeten parallel efficient gebeuren.
//       // Kunnen we op basis van de rekenmodellen zoals die in excel uitgeprogrammeerd zijn vertalen naar efficiente code die voor alles goed zijn?
//       // Niet hele complexe formules.
//       // Veel throughput.
//       // GPU computing?
//       // Floating point and financial is a no-go.
//       // Hoe ocmpilen we excel functies naar echte code, maar dan geoptimaliseerd voor de use cases van pensioen.
//       // Bepalen wat pensioen tot nu is is niet heel moeilijk
//       // Prognose berekening
    
//       // InfoSupport krijgt requirements: stuur actuariele engine aan.
//       // Als pensioen sneller moet kunnen, moet het snel aangepast kunnen worden.
//       // Als ik ga scheiden, wat is de impact.
    
//       // Parallelisation?
    
//       // Rekenpakket wordt 1-op-1 overgezet.
//       // Excel parsen en sneller maken
//       // Uit excel trekken, hoe optimaliseren we de rekenkracht.
//       // Optimalisatietechnieken
    
//       // uitdagingen 
//       // -> floating point errors.
//       // -> Circulaire referentie
//       // -> Implementatie van formules
//       // -> VBA functions
//       // Excel? -> Formules niet met de hand overtypen.
    
//       // COmpilatie
//       // -> Belangrijk: De code die uiteindelijk uit excel komt moet rekening houden met context
  
// + How complex are the formulas in the Excel sheets?
//   + Do you have an example of a complex formula?
//   + Are dynamic ranges used, such as INDEX or OFFSET?
//   + Are VBA functions used? If so, how difficult is it to convert them, and why?

// + Are optimizations already being applied during the conversion process, such as the use of parallelization? If so, what challenges arise in this process?

// + How clean is the data in the Excel sheets? Are errors common?

// + Is it possible to access an (anonymized) Excel sheet? And would I be allowed to publish it?
// // -> Use datasets of government. 

// // Peter is heel inhoudelijk

// // Gebruik datasets van overheid om te kijken wat hetzelfde resultaat is.

// // Give a short (at most 1 page) description of the project. Make sure the context of the problem. From the introduction it should be immediately clear how your proposed contribution is scientifically relevant and could fill the research gap.

// // Towards the end of the introduction, you should also add your preliminary #strong[research questions (RQ)];. You may want to state your main RQ like this:

// // #quote(
// //   block: true,
// // )[
// //   Can a master thesis proposal serve as a starting point for a Nobel Prize?
// // ]

// // And your sub-RQs like this:

// // - #strong[RQ1] This is what a first research question looks like
// // - #strong[RQ2] This is what a second research question looks like
// // - #strong[RQ3] This is what a third research question looks like
// //   - #strong[RQ3.1] This is what a sub-research question looks like

= Related Work <related-work>
While little to no literature exists on the conversion of spreadsheets to executable programs, the paradigm of converting loosely based spreadsheets to a more structured format has been exhaustively researched.

#citeauthor(<cunha_spreadsheet_2015>) propose the term spreadsheet engineering, which involves 



// Discuss the literature related to your proposal. Focus on concepts and ideas, how this relates to your proposal. Do not describe each paper individually, but as a collective.

// Aim for a "matrix" structure of the related work: What are the features desired from the outcome of your research? How are these features delivered by existing work? Are there gaps unanswered by existing work?

// You can use BibLaTeX to keep track of your references in a separate file (`references.bib` in this template). To cite a paper, use `\cite{key}`, for example `\cite{ISO25010}`, becomes @ISO25010. You can also directly cite the author by using `\citeauthor{key}` and `\citetitle{key}` for the title. For example:

// #quote(block: true)[
// `As \citeauthor{Beck1999} shows in \citetitle{Beck1999}~\cite{Beck1999}`
// ]

// becomes

// #quote(block: true)[
//   As #cite(<Beck1999>, form: "prose") shows in ~@Beck1999
// ]

// A guideline is to include between 10 and 20 papers on your topic in the related work. The exact number depends on the topic and available literature.

= Methodology <methodology>
// Present your approach, how you are going to find the answers to your research question. This section should cover answers to the questions:

// - What will make the research difficult?

// - What is the input you expect from the literature survey

// - What sources will you use and how will you use / document them?

// - What experiments / research will you do? What proof of concept will you make?

// - What research method will you use?

// - Which hypothesis do you have?

// - Present a time line, how results from each step feeds into the next step and ultimately answers your research questions.

// - How will you validate your research?

= Risk Assessment <risk-assessment>
This section outlines potential risks associated with this project and proposes mitigation strategies. A
comprehensive summary of each identified risk is
presented in @tab:risk-register.

#figure(
    table(
        columns: 5, 
        align: (left, center, center, center, left,), 
        table.header([*Description*], table.cell(align: center, colspan: 3)[*Risk*], [*Action*]), 
        [], [Likelihood], [Impact], [Score], [], 
        [Scope creep], [5], [5], [25], [Regular meetings with supervisor to calibrate project goals], 
        [Supervisor goes on holiday], [1], [1], [1], [Add holidays into planning], 
        [Laptop gets stolen], [1], [5], [5], [Make regular backups of experiment data, list of loaner laptops], 
        [Project planning errors], [3], [3], [9], [Build in 10% contingency, track and review progress every week], 
        [Lack of communication, causing delays], [1], [1], [1], [Send weekly updates to supervisor with progress, blockers, and plans for next week],
      ), 
      caption: [Risk Register],
  )
<tab:risk-register>

== Information about the project is not easily accessible.
At this step in the requirements gathering process, communication with Info Support B.V. stakeholders (excluding the supervisors) are slow and inefficient. Should this trend continue, it could potentially delay progress when information is needed on the project. Based on the current trend, I would classify this as likely and highly impactful on the research.

To mitigate this, in-person impromptu meetings with relevant stakeholders should be held to facilitate efficient information exchange. Additionally, allocating extra time for information
gathering throughout the project timeline is essential.

== Project goals are too ambitious
The project's scope may be too ambitious for a master thesis, potentially leading to an unrealistic workload and compromised quality. Given previous projects, this is the biggest risk, scoring a high likelihood and high impact. 

That said, this risk can be mitigated by selecting a more specific scope, and regularly meeting with supervisors to critically evaluate project goals and adjust them if necessary. Furthermore, planning for potential adjustments and building in extra time to accommodate unforeseen challenges will be crucial.

== Holidays
While there should be no holiday breaks between now and the projected finish date, when the project is delayed a pre-planned holiday throughout the month of August presents a challenge for late submission, resulting in an even later submission. While this is a risk that only poses a threat if the project gets delayed, it has a large impact, but a very low likelihood.

To mitigate this, the research will begin earlier than usual to gain a head-start on a traditional planning. Additionally, a 10% additional contingency time within the schedule will be planned to accommodate potential delays.

== Unavailability of supervisors
Unforeseen circumstances could lead to difficulties scheduling meetings with supervisors, hindering guidance and
feedback. This can be mitigated by proactively planning meetings with supervisors well in advance to secure their
availability.

== Minor risks
The following risks are minor risks and have a low likelihood or impact.

Unexpected illness could lead to disruptions and delay project progress. This risk can be minimized by
prioritizing a healthy work-life balance and avoiding overwork to minimize burnout risk. A 10% contingency time
buffer will also be built into the schedule to accommodate potential sickness.

Data loss due to laptop theft is a low-probability risk that still requires mitigation. Implementing robust data
backup strategies including both automatic and manual cloud backups will be essential. Regularly storing code in
a version control system like Git and committing changes frequently will also be crucial.

Despite careful planning, unforeseen errors or omissions can occur. This risk can be mitigated by conducting
weekly progress reviews with supervisors to identify potential issues early on. Maintaining a 10% contingency
buffer within the schedule will also help address unexpected challenges.

Difficulty finding motivation and focusing on writing tasks can be overcome through various techniques such as
brainstorming sessions, freewriting exercises, and seeking feedback from peers or mentors.

= Ethics <ethics>
There are no foreseeable ethical doubts or dillemas on all areas. The project will not involve human research, data sets about people or cybersecutiry or online privacy issues. While we could use an LLM like Codex to make the code more 'human readable', this does not pose any threats. No sensitive or personal data is involved. Additionally, the outputs are fully explainable and align with the practices of experienced developers, mitigating any risks of misinterpretation or unintended consequences. As such, no ethical doubts or dilemmas are anticipated in any area of the project.

// Fill out the ethics self-check via the RMS portal for Faculty of Science#footnote[#link("https://rms.uva.nl/servicedesk/customer/portal/14");];, and present a summary of the result and future actions. The main areas for ethical concerns are:

// - Research that includes active involvement of human research participants and/or gathering of new data from human participants

// - Research that involves the inclusion, combination, use, and/or analysis of already existing data sets about people

// - Design or development of AI-technologies/algorithms, or deployment and/or use of AI-technologies/algorithms for practical applications

// - Research that involves cyber security and/or online privacy issues

// If the self-check does not lead to an adequate answer to doubts and dilemmas, consult the Ethical Review Committee (ECIS-SP for SE students) via the Ethics portal.

= Project Plan <project-plan>
Describe your project plan and present it via Gantt charts or a timeline. You can see an example in~#link(<fig:gantt-chart>)[1];. Make sure you describe achievements, not actions, instead of "prototype development" you write "implemented first version of prototype in COBOL, ready for evaluation by user group".

#figure(
  timeliney.timeline(
  show-grid: true,
  {
    import timeliney: *
    
    let dt(month, day) = datetime(year: 2025, month: month, day: day)
    
    let calcTime(start, end) = {
      return int((end - start).days())
    }
    
    let at(date) = {
      return int((date - startDate).days())
    }

    let startDate = datetime(year: 2025, month: 2, day: 7)
    let endDate = dt(7,21)
    
    
    headerline(
      group(([*Feb*], calcTime(dt(2,3), dt(2,28)))),
      group(([*Mar*], calcTime(dt(2,28), dt(3,31)))),
      group(([*Apr*], calcTime(dt(3,31), dt(4,30)))),
      group(([*May*], calcTime(dt(4,30), dt(5,31)))),
      group(([*June*], calcTime(dt(5,31), dt(6,30)))),
      group(([*July*], calcTime(dt(6,30), endDate))),
    )
    
    headerline(
      group(([*Block 4*], calcTime(dt(2, 3), dt(3, 31)))), 
      group(([*Block 5*], calcTime(dt(3, 31), dt(6, 2)))), 
      group(([*Block 6*], calcTime(dt(6, 2), dt(6, 30)))),
      group(([], calcTime(dt(6, 30), endDate)))
    )

    headerline(
      group(..range(8).map(n => ([#(n+1)], 7))),
      group(..range(9).map(n => ([#(n+9)], 7))),
      group(..range(4).map(n => ([#(n+18)], 7))),
      group(..range(3).map(n => ([#(n+22)], 7))),
    )
    
    taskgroup(title: [*Research*], {
      task("Research the market", (0, 2), style: (stroke: 2pt + gray))
      task("Conduct user surveys", (1, 3), style: (stroke: 2pt + gray))
    })

    taskgroup(title: [*Development*], {
      task("Create mock-ups", (2, 3), style: (stroke: 2pt + gray))
      task("Develop application", (3, 5), style: (stroke: 2pt + gray))
      task([QA], (3.5, 6), style: (stroke: 2pt + gray))
    })

    taskgroup(title: [*Academic*], {
      task("Writing of thesis", (3.5, 7), style: (stroke: 2pt + gray))
      task("Social media advertising", (6, 7.5), style: (stroke: 2pt + gray))
    })

    milestone(
      at: 3.75,
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
])
<fig:gantt-chart>

#bibliography("references.bib", full: false, style: "ieee")