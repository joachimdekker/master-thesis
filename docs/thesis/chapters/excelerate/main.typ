#import "../../utils/chapters.typ": chapter
#import "../../utils/spreadsheet.typ": spreadsheet
#import "../../utils/cite-tools.typ": citeauthor

// Setup 

#show raw.where(block: false): it => {
    set text(font: "JetBrains Mono", size: 0.9em)
    
    box(it, 
    fill: luma(95%), 
    inset: (x: 4pt, y: 0pt),
    outset: (y: 4pt),
    radius: 2pt,)
}

#show figure: it => {
  show raw.where(block: true): r => {
    set text(font: "JetBrains Mono", size: 0.7em)
  
    // The raw block should be encased in a box
    align(left,
      block(r, fill: luma(97%), inset: 1em, radius: .25em, width: 100%)
    )
  }

  show raw.where(block: false): r => {
    set text(font: "JetBrains Mono", size: 1em)
    r
  }
  
  it
}


#chapter("Excelerate")

= Structures

= Changes to the Compiler

= Finding the Structures

= Embedding the Structures

= Coding the Structures