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
        [Project planning errors], [3], [3], [9], [Build in 10% contingency, track and review progress every week], 
        [Lack of communication, causing delays], [1], [1], [1], [Send weekly updates to supervisor with progress, blockers, and plans for next week],
        [Stakeholder communication delays], [4], [4], [16], [Hold in-person impromptu meetings, allocate extra time for information gathering], 
        [Ambitious project goals], [5], [5], [25], [Select a specific scope, meet with supervisors regularly, build in extra time], 
        [Holiday delays], [1], [10], [10], [Begin research earlier, add 10% contingency buffer], 
        [Unavailability of supervisors], [1], [3], [3], [Proactively schedule meetings well in advance], 
        [Unexpected illness], [2], [3], [6], [Maintain a healthy work-life balance, build in 10% contingency buffer], 
        [Unforeseen errors or omissions], [3], [3], [9], [Conduct weekly progress reviews, maintain a 10% contingency buffer], 
        [Items gets stolen or data corruption], [1], [5], [5], [Make regular backups of data and use version-control for code],
        [Motivation and focus challenges], [2], [3], [6], [Use brainstorming, freewriting, and seek feedback],
      ), 
      caption: [Risk Register],
      placement: auto,
  )
<tab:risk-register>

== Information about the project is not easily accessible.
At this step in the requirements gathering process, communication with Info Support B.V. stakeholders (excluding the supervisors) are slow and inefficient. Should this trend continue, it could potentially delay progress when information is needed on the project. Based on the current trend, I would classify this as likely and highly impactful on the research.

To mitigate this, in-person impromptu meetings with relevant stakeholders should be held to facilitate efficient information exchange. Additionally, allocating extra time for information
gathering throughout the project timeline is essential.

=== Dataset is insufficient <subsec:insufficient-data>
The current dataset, while huge, is too simplistic to perform experiments on. There is a lot of validation data that we can use to validate the compilation, but the formulas that are used in the dataset are too simple. A bigger dataset should be collected or constructed, but this could add extra time. To mitigate this, we will use the first couple of weeks to find a dataset, but if it is not possible, we will create smaller datasets ourselves.

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
