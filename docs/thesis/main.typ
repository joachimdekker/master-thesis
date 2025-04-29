#include "frontpage.typ"
#include "abstract.typ"

#import "utils/chapters.typ": chapter

#text([Contents], weight: "bold", size: 2em)
#v(2em)
#outline(title: none, indent: auto, depth: 3)
#pagebreak(weak: true)

#let x = context if (not query(heading.where(level: 1).after(here()))
      .map(h => h.location().page())
      .at(0, default: 0) == here().page()) { 0 }

// Make sure chapters are level 1 heading and other headings get level 2
#set heading(numbering: "1.", offset: 1)
#set page(numbering: "1")

#show ref: it => if it.element == none or it.element.func() != heading or it.element.level != 1 { it } else {
  let l = it.target // label
  let h = it.element // heading

  let format = if h.numbering.ends-with(".") {h.numbering.slice(0,-1)} else {h.numbering}
  
  let number = numbering(
      format,
      ..counter(heading).at(l))
  link(l, [Chapter #number])
}

// Make lvl 2 headings bigger since they should be lvl 1
#show heading.where(level: 2): set text(size:1.5em)
#show heading.where(level: 3): set text(size:1.3em)

// Chapter page
#show heading.where(level: 1): it => {
  pagebreak(weak: true)
  let number = context counter(heading).get().at(0)
  place(bottom, float: false, dy: -0em, dx: -2em, scope: "column", text(number, size: 30em, fill: gray))
  let content = [
    #set align(right)
    #text([_Chapter #number _], weight: "regular", size: 2em)
    #v(-2.5em)
    #text(it.body, size: 3em)
  ]
  
  let topLevelOutline = context {
    let sections = heading.where(level: 2).after(here(), inclusive: false)
    let chapterNr = counter(heading).get().at(0)
    if chapterNr != counter(heading).final().at(0) {
      let nextChapter = query(heading.where(level: 1).after(here(), inclusive: false)).at(0)
      sections = sections.before(nextChapter.location())
    } else {
      let bib = query(bibliography.where().after(here())).at(0)
      sections = sections.before(bib.location())
    }
    v(2em)
    outline(title: none, target: sections, indent: 50%)
  }
  
  layout(size => {
    let half = 50% * size.height
    let pageHalf = 50% * page.height

    let titleHeight = measure(content).height
    let outlineHeight = measure(topLevelOutline).height
    
    v(half - (titleHeight + outlineHeight) / 2)
  })
  
  content
  

  topLevelOutline
}

#include "chapters/introduction/main.typ"

#include "chapters/Compiling Excel/main.typ"

#pagebreak()
#bibliography("zotero.bib", title: [Bibliography])