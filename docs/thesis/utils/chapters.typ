#let chapterLabel() = label("chapter-" + counter(heading).get().at(0))


#let chapter(name, break-page: true) = {
  [#heading(name, offset: 0, numbering: "1.", )#label("chapter-" + lower(str(name)).replace(" ", "-"))]
  if (break-page) { pagebreak(weak:true) }
}
