#let chapterLabel() = label("chapter-" + counter(heading).get().at(0))


#let chapter(name) = {
  [#heading(name, offset: 0, numbering: "1.", )#label("chapter-" + lower(str(name)).replace(" ", "-"))]
  pagebreak(weak:true)
}
