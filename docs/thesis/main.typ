#include "frontpage.typ"
#include "abstract.typ"

#set text(region: "GB")

#import "@preview/wordometer:0.1.4": word-count, total-words

// #show: word-count
// #total-words

#import "utils/chapters.typ": chapter

#text([Contents], weight: "bold", size: 2em)
#v(2em)
#outline(title: none, indent: auto, depth: 3)
#pagebreak(weak: true)

#set par(
  justify: true,
)

// #let x = context if (not query(heading.where(level: 1).after(here()))
//       .map(h => h.location().page())
//       .at(0, default: 0) == here().page()) { 0 }

// Make sure chapters are level 1 heading and other headings get level 2
#set heading(numbering: "1.", offset: 1)
#set page(numbering: "1")

#show figure: it => {
  show raw.where(block: true): r => {
    set text(font: "JetBrains Mono", size: 0.7em)
  
    // The raw block should be encased in a box
    align(left,
      block(r, fill: luma(97%), inset: 1em, radius: .25em, width: 100%)
    )
  }
  it
}

#show ref: it => if it.element == none or it.element.func() != heading or it.element.level != 1 { it } else {
  let l = it.target // label
  let h = it.element // heading

  let format = if h.numbering.ends-with(".") {h.numbering.slice(0,-1)} else {h.numbering}
  
  let number = numbering(
      format,
      ..counter(heading).at(l))
  link(l, [Chapter #number])
}

#show ref: it => if it.element == none or it.element.func() != heading or it.element.level <= 3 { it } else {
  let l = it.target
  let h = it.element

  let count = numbering("1.1", ..counter(heading).at(l).slice(0, 3))

  link(l, [Section #count])
}

// Make lvl 2 headings bigger since they should be lvl 1
#show heading.where(level: 2): it => text(it, size:1.5em)

#show heading.where(level: 3): set text(size:1.5em)

#show heading.where(level: 4): it => text([#it.body #v(-0.5em)], size: 1.3em)

#show heading.where(level: 5): it => {
  let body = it.body.text
  if (not body.ends-with(".")) {
    body = body + "."
  }
  
  [_#body #h(1cm)_]
}

// Chapter page
#show heading.where(level: 1): it => {
  pagebreak(weak: true)
  let number = context counter(heading).get().at(0)
  place(bottom, float: false, dy: -0em, dx: -2em, scope: "column", text(number, size: 30em, fill: gray))
  let content = [
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
    set align(left)
    outline(title: none, target: sections, indent: 50%)
  }
  
  set align(horizon + right)
  content
  topLevelOutline
  

}

#show raw: set text(font: "JetBrains Mono")

#show "Excelerate": smallcaps
#show "Csharp": "C#"

// ========================================================== //

// There are #total-words no words.

#include "chapters/introduction/main.typ"

#include "chapters/basic-compiler/main.typ"

#include "chapters/excelerate/main.typ"

#include "chapters/compiling-excel/main.typ"

#include "chapters/evaluation/main.typ"

#include "chapters/conclusion/main.typ"

#pagebreak()
#bibliography("zotero.bib", title: [Bibliography], style: "short-cite.csl")

