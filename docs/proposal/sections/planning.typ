#import "@preview/timeliney:0.1.0"

= Project Plan <sec:project-plan>
We present the plan to conduct the research and write the thesis in this section. An overview of the timeline is presented in @fig:gantt-chart. In the rest of the section, we will discuss the  We propose to write the thesis alongside the research, since the research will be incremental.

== Context
The research will be conducted in two phases. Phase one, consisting of block 4, will be the preparatory phase, where the initial domain model and _simple_ code generation will be created. Only one day per week (at Info Support) will be devoted to the first phase, given the remaining courses during block 4.

In phase two, we will be working on the thesis full-time, devoting the full week. As such, the timeline from block 5 is a lot more busy than in phase one. During the second phase, we will begin begin researching the difficult parts such as the mutual recursion and further optimalizations in the generation of the code. 

#figure(
  timeliney.timeline(
    show-grid: false,
    {
      import timeliney: *
      
      let dt(month, day) = datetime(year: 2025, month: month, day: day)
      
      let startDate = datetime(year: 2025, month: 2, day: 3)
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
      
      taskgroup(title: align(left, [*Research*]), {
        task([Domain Model for \ Simple Mappings], weeks(1, 4), style: (stroke: 2pt + gray))
        task([Code Generation], weeks(4, 8), style: (stroke: 2pt + gray))
        task([Generation of \ Cyclic References \ and Mutual Recursion ], weeks(9, 12), style: (stroke: 2pt + gray))
        task([Optimalizations in generation], weeks(9, 16), style: (stroke: 2pt + gray))
      })
  
      taskgroup(title: [*Validation*], {
        task([Test Setup], weeks(6, 8), style: (stroke: 2pt + gray))
        task([Empirical verification of semantics], weeks(9, 14), style: (stroke: 2pt + gray))
        task([Comparison of performance], weeks(9, 16), style: (stroke: 2pt + gray))
      })
  
      taskgroup(title: [*Thesis Writing*], {
        task([Draft], weeks(9, 16), style: (stroke: 2pt + gray))
        task([Release Candidate], weeks(17, 19), style: (stroke: 2pt + gray))
        task([Final Version], weeks(20, 21), style: (stroke: 2pt + gray))
      })
  
      milestone(
        at: at(3,29),
        style: (stroke: (dash: "dashed")),
        align(center, [
          *Alpha Release*\
          March 29th
        ])
      )
  
      milestone(
        at: at(5,23),
        style: (stroke: (dash: "dashed")),
        align(center, [
          *Beta Release*\
          March 29th
        ])
      )
  
      milestone(
        at: at(7,1),
        style: (stroke: (dash: "dashed")),
        align(center, [
          *Hand-in*\
          Early July
        ])
      )
  
      milestone(
        at: at(7,19),
        style: (stroke: (dash: "dashed")),
        align(center, [
          *Defense*\
          Mid-July or September
        ])
      )
    }
  ), caption: [
    A rough sketch of the planning of the research project.
  ],
  supplement: "Timeline",
placement: auto
)
<fig:gantt-chart>

== Research
The research starts with the creation of a simple domain model for the Excel mappings and the generation of the code thereof. This research is to prepare the next phase and will take place in febuari and march. At the end of the first phase, we want to release an _alpha_ version of the tool that can compile simple excel worksheets to code. Afterwards, we will look for optimisations in the tool to compare the efficiency of the tool during Block 5. During that time, we will also extend the domain to support cyclic references and look for the optimal way to represent them. At the end of may, we plan to release the _beta_ version, which should also support the cyclic references.

== Validation
To research the validity of the program and thesis, we will perform an empirical testing approach. We want to set-up the testing setup at the end of march, which will help the comparison and testing of the methods applied from the research. For the formal verification, we focus for a month on defining a technique to validate the compilation, and formalize the semantics of the excel formulas. Furthermore, during the formal verification and the research of the optimalizations, we will be performing performance tests to compare the methods, but this will take relatively little time in comparison to the other tasks.

== Thesis Writing
Thesis writing is planned alongside research and validation, ensuring it aligns with the current progress and research. We will begin writing the draft in March, and update it incrementally, ensuring a good draft at the end of May. Afterwards, based on feedback from the supervisors, a release candidate will be created, incorporating the feedback. After a final round of formal feedback, the thesis will be finalized and ready for hand-in by early July.

