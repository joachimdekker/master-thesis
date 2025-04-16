#include "frontpage.typ"
#include "abstract.typ"

#import "utils/chapters.typ": chapter

#text([Contents], weight: "bold", size: 2em)
#v(2em)
#outline(title: none, indent: auto, depth: 3)
#pagebreak(weak: true)

// Make sure chapters are level 1 heading and other headings get level 2
#set heading(numbering: "1.", offset: 1)

#show ref: it => if it.element.func() != heading or it.element.level != 1 { it } else {
  let l = it.target // label
  let h = it.element // heading

  let format = if h.numbering.ends-with(".") {h.numbering.slice(0,-1)} else {h.numbering}
  
  let number = numbering(
      format,
      ..counter(heading).at(l))
  link(l, [Chapter #number])
}

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
  v(50% - (measure(content).height))
  content

  v(2em)
  context {
    let sections = heading.where(level: 2).after(here(), inclusive: false)
    let chapterNr = counter(heading).get().at(0)
    if chapterNr != counter(heading).final().at(0) {
      let nextChapter = query(heading.where(level: 1).after(here(), inclusive: false)).at(0)
      sections = sections.before(nextChapter.location())
    }
    outline(title: none, target: sections, depth: 2, indent: 50%)
  }
}

#include "chapters/introduction/main.typ"