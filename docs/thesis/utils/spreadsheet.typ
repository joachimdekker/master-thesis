#let spreadsheet(
  columns: 3,
  ..content
) = {
  let border(t) = table.cell(text(t, fill: luma(80%), weight: "bold", size: 0.7em), fill: black, inset: 0.5em)

  let chars = ("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K")

  let topRow = range(columns).map(it => {border(chars.at(it))})
  topRow.insert(0, border([]))

  
  let sheetArray = content.pos()
  let count = sheetArray.len()
  let rowCount = calc.div-euclid(count, columns);
  
  let ri = 1
  for i in range(0, count + rowCount,  step: columns + 1) {
    sheetArray.insert(i, border([#ri]))
    ri += 1
  }
  
  let tableArgs = topRow + sheetArray
  
  return table(columns: columns + 1, ..tableArgs)
}