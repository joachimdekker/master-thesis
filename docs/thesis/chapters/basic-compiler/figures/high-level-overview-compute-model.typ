#import "@preview/fletcher:0.5.8" as fletcher: diagram, node, edge

#figure(
  {
    let excelGreen = rgb("#257835")
    let uvaRed = rgb("#BC0031")

    set text(fill: white, weight: "bold", size: 1em)
    // scale(
    diagram(
      edge-stroke: 1pt,
      node-corner-radius: 5pt,
      edge-corner-radius: 8pt,
      mark-scale: 60%,
  
      node((0,0), [*Sum*], name: <sum>, fill: excelGreen, shape: fletcher.shapes.hexagon),
  
      node((1.3,1), [-], name: <minus-1>, fill: orange, shape: fletcher.shapes.circle),
      node((0,1), [-],  name: <minus-2>, fill: orange, shape: fletcher.shapes.circle),
      node((-1.3,1), [-],  name: <minus-3>, fill: orange, shape: fletcher.shapes.circle),
  
      node((1.65, 2), [6000], name: <minus-1-arg1>, fill: uvaRed),
      node((1, 2), [5800], name: <minus-1-arg2>, fill: uvaRed),
      
      node((-0.3, 2), [1000], name: <minus-2-arg1>, fill: uvaRed),
      node((0.3, 2), [2300], name: <minus-2-arg2>, fill: uvaRed),
      
      node((-1.65, 2), [2500], name: <minus-3-arg1>, fill: uvaRed),
      node((-1, 2), [1500], name: <minus-3-arg2>, fill: uvaRed),
      
      edge(<minus-1>, <sum>, "-}>"),
      edge(<minus-2>, <sum>, "-}>"),
      edge(<minus-3>, <sum>, "-}>"),
      
      
      edge(<minus-3-arg1>, <minus-3>, "-}>"),
      edge(<minus-3-arg2>, <minus-3>, "-}>"),
      edge(<minus-1-arg1>, <minus-1>, "-}>"),
      edge(<minus-1-arg2>, <minus-1>, "-}>"),
      edge(<minus-2-arg1>, <minus-2>, "-}>"),
      edge(<minus-2-arg2>, <minus-2>, "-}>"),    
    )
  },
  caption: [The compute graph generated from the formulae and dependencies of @sps:budget:income:formulae.],
  placement: auto
)<fig:high-level-overview:compute-graph>