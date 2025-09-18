#let spreadsheet(
  columns: 3,
  dependencies: (),
  hasHeader: false,
  hasFooter: false,
  hasTitle: false,
  ..content
) = {
  show raw: r => {
    set text(font: "JetBrains Mono", size: 1em)
    r.text
  }
  
  let border(t) = table.cell(text(t, fill: luma(20%), weight: "bold", size: 0.6em), fill: luma(80%), inset: (x:0.5em, y:0.3em), align: center + horizon, )

  let chars = ("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K")

  let topRow = range(columns).map(it => {border(chars.at(it))})
  topRow.insert(0, border([]))

  
  let sheetArray = content.pos()
  let count = sheetArray.len()
  let rowCount = calc.div-euclid(count, columns) + if (calc.rem(count, columns) == 0) { 0 } else { 1 };
  
  let ri = 1
  for i in range(0, count + rowCount,  step: columns + 1) {
    sheetArray.insert(i, border([#ri]))
    ri += 1
  }
  
  let tableArgs = topRow + sheetArray

  if dependencies != () {
    
  }

  let spreadsheetTable = table(columns: columns + 1, ..tableArgs)

  let beginOffset = if (hasHeader and hasTitle) { 2 } else if (hasHeader or hasTitle) { 1 } else { 0 }
  let endOffset = if (hasFooter) { 1 } else { 0 }
  
  set table(
    stroke: 1pt + luma(80%)
    // (x, y) => (
    //   top: if (y == 0 ) {1pt + luma(80%)},
    //   bottom: if (x == 0) { 1pt + luma(80%) } else { 1pt + luma(80%) },
    //   left: if (y == 0 or x == 0) { 1pt + luma(80%) },
    //   right: if (y == 0) { 1pt + luma(80%) } else { 1pt },
    // ),
  )

  show table.cell: set align(left)
  
  block(
    stroke: none,
    [
      #spreadsheetTable
    ]
  )
  
}