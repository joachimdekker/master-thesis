#let chapterLabel() = label("chapter-" + counter(heading).get().at(0))


#let chapter(name, break-page: true) = {
  show heading.where(level: 1): it => {
    pagebreak(weak: true)
    let number = context counter(heading).get().at(0)
    place(bottom, float: false, dy: -0em, dx: -2em, scope: "column", text(number, size: 30em, fill: gray))
    let content = [
      #text([_Chapter #number _], weight: "regular", size: 2em)
      #v(-2.5em)
      #text(it.body, size: 3em)
    ]
    
    let topLevelOutline = context {
      let bib = query(bibliography.where().after(here())).at(0)
      let sections = heading.where(level: 2).after(here(), inclusive: false)
      let chapterNr = counter(heading).get().at(0)
      
      if chapterNr != counter(heading).at(bib.location()).at(0) {
        let possibleNextChapter = query(heading.where(level: 1).after(here(), inclusive: false))
        let nextChapter = possibleNextChapter.at(0)
        sections = sections.before(bib.location()).before(nextChapter.location())
      } else {        
        let bib = query(bibliography.where().after(here())).at(0)
        sections = sections.before(bib.location())
      }
      v(2em)
      set align(left)
      outline(title: none, target: sections, indent: 50%)
    }
  
    counter(figure.where(kind: image)).update(0)
    counter(figure.where(kind: table)).update(0)
    counter(figure.where(kind: raw)).update(0)
    counter(math.equation).update(0);
    counter(figure.where(kind: "spreadsheet")).update(0)
    
    set align(horizon + right)
    content
    topLevelOutline
  }
  
  [#heading(name, offset: 0, numbering: "1.", )#label("chapter-" + lower(str(name)).replace(" ", "-"))]
  
  if (break-page) { pagebreak(weak:true) }
}
