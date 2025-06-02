#import "../../../utils/spreadsheet.typ": spreadsheet 

#figure(
  spreadsheet(
    columns: 4,
    [], [Projected], [Actual], [Difference],
    [Income 1], [\$ 6.000], [\$ 5.800], [-\$ 200],
    [Income 2], [\$ 1.000], [\$ 2.300], [\$ 1.300],
    [Extra Income], [\$ 2.500], [\$ 1.500], [-\$ 1.000],
    [TOTAL], [], [], [\$ 100]
  ),
  caption: [The Income Summary section of the Family Budget spreadsheet. The project and actual income are raw inputs, and the cells in the difference column calculate the difference between the projected and actual income.],
  supplement: "Spreadsheet",
  placement: auto,
)<sps:budget:income>

#figure(
  spreadsheet(
    columns: 4,
    [], [Projected], [Actual], [Difference],
    [Income 1], [6.000], [5.800], [=C2-B2],
    [Income 2], [1.000], [2.300], [=C3-B3],
    [Extra Income], [2.500], [1.500], [=C4-B4],
    [TOTAL], [], [], [=SUM(D2:D4)]
  ),
  caption: [The formulae behind @sps:budget:income. It is clear that there is a relationship between the Projected and Actual column of this table. ],
  supplement: "Spreadsheet",
  placement: top,
)<sps:budget:income:formulae>