using System.Globalization;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Structure;
using Irony.Parsing;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using XLParser;
using Range = ExcelCompiler.Domain.Structure.Range;
using TableReference = ExcelCompiler.Domain.Structure.TableReference;

namespace ExcelCompiler.Passes;

public class FrontendPass
{
    private Dictionary<string, Reference> _namedRanges = [];
    
    public Workbook Transform(Stream excelFile)
    {
        using var p = new ExcelPackage(excelFile);
        
        // Convert the named ranges
        // _namedRanges = p.Workbook.Names
        //     .ToDictionary(namedRange => namedRange.Name, namedRange => Reference.Parse(namedRange.Address));
        
        Workbook workbook = new Workbook(name: "ExcelWorkbook")
        {
            NamedRanges = _namedRanges,
        };
        
        // Get the spreadsheets of the excel file
        foreach (var sheet in p.Workbook.Worksheets)
        {
            Spreadsheet spreadsheet = new Spreadsheet(name: sheet.Name);
            
            // Convert all the cells in the spreadsheet
            foreach (var cell in sheet.DimensionByValue)
            {
                // Convert the cell.
                Location location = Location.FromA1(cell.Address, spreadsheet.Name);
                ComputeUnit? computeUnit = GetComputeUnit(location, p);
                Cell? newCell = computeUnit switch
                {
                    ConstantValue<string> @string => new ValueCell<string>(location, @string.Value),
                    ConstantValue<double> @double => new ValueCell<double>(location, @double.Value),
                    ConstantValue<decimal> @decimal => new ValueCell<decimal>(location, @decimal.Value),
                    ConstantValue<bool> @bool => new ValueCell<bool>(location, @bool.Value),
                    Function f => new FormulaCell(location, f),
                    _ => null,
                };
                
                if (newCell is not null)
                {
                    spreadsheet[location] = newCell;
                }
            }
            
            
            // Get all the tables in the excel file
            foreach (ExcelTable excelTable in sheet.Tables)
            {
                Range tableRange = (Reference.Parse(excelTable.Range.Address) as Range)!;
                // Temp to fix the spreadsheet linkage problem.
                tableRange.From.Spreadsheet = spreadsheet.Name;
                tableRange.To.Spreadsheet = spreadsheet.Name;
                
                Dictionary<string, Range> columns = excelTable.Columns.ToDictionary(col => col.Name, col =>
                {
                    int column = tableRange.From.Column + col.Position;
                    int startRow = excelTable.ShowHeader ? tableRange.From.Row + 1 : tableRange.From.Row;
                    int endRow = excelTable.ShowTotal ? tableRange.To.Row - 1 : tableRange.To.Row;
                    
                    return new Range(
                        from: new Location()
                        {
                            Spreadsheet = tableRange.From.Spreadsheet,
                            Row = startRow,
                            Column = column,
                        },
                        to: new Location()
                        {
                            Spreadsheet = tableRange.To.Spreadsheet,
                            Row = endRow,
                            Column = column,
                        }
                    );
                });
                
                Table table = new Table(excelTable.Name)
                {
                    Columns = columns,
                    Location = tableRange,
                };
                
                spreadsheet.Tables.Add(table);
            }
            
            workbook.Spreadsheets.Add(spreadsheet);
        }

        return workbook;
    }

    private ComputeUnit? GetComputeUnit(Location cellLocation, ExcelPackage excelFile)
    {
        var cell = excelFile.Workbook.Worksheets[cellLocation.Spreadsheet].Cells[cellLocation.Row, cellLocation.Column];
            
        // Check if the cell contains a value or a formula
        bool isFormula = cell.Formula is not null and not "";
        if (!isFormula && cell.Value is null or "") return null!;
            
        if (!isFormula)
        {
            ComputeUnit valueCell = cell.Value switch
            {
                string str => new ConstantValue<string>(str, cellLocation),
                double d => new ConstantValue<double>(d, cellLocation),
                decimal d => new ConstantValue<decimal>(d, cellLocation),
                bool b => new ConstantValue<bool>(b, cellLocation),
                DateTime dt => new ConstantValue<DateTime>(dt, cellLocation),
                _ => throw new ArgumentException("Unsupported cell value type.", nameof(cell))
            };
            
            return valueCell;
        }

        ParseTreeNode node = ExcelFormulaParser.Parse(cell.Formula!);
        
        // Convert the parse tree node to a formula
        ComputeUnit formula = ConvertParseTreeToFormula(node, cellLocation);
        return formula;
    }

    private ComputeUnit ConvertParseTreeToFormula(ParseTreeNode node, Location location)
    {
        if (node.IsParentheses()) return ConvertParseTreeToFormula(node.ChildNodes[1], location);
        
        return node.Type() switch
        {
            GrammarNames.Formula => ConvertParseTreeToFormula(node.ChildNodes[0], location),
            GrammarNames.FunctionCall => ParseFunctionCall(node, location),
            GrammarNames.Text => new ConstantValue<string>(node.Token.ValueString, location),
            GrammarNames.Number => new ConstantValue<decimal>(decimal.Parse(node.Token.ValueString), location),
            GrammarNames.Reference => ParseReference(node, location),
            GrammarNames.UDFunctionCall => throw new ArgumentException(
                "User-defined functions are not supported at this time.", nameof(node)),
            _ => node switch
            {
                { ChildNodes.Count: 1 } childNode => ConvertParseTreeToFormula(childNode, location),
                _ => throw new ArgumentException("Unsupported parse tree node.", nameof(node))
            }
        };
    }


    private ComputeUnit ParseReference(ParseTreeNode node, Location location)
    {
        // if (node.IsRange())
        // {
        //     return new FunctionComposition("Range", [
        //         ParseReferenceItem(node.ChildNodes[0], location),
        //         ParseReferenceItem(node.ChildNodes[2], location)
        //     ], location);
        // }
        
        return ParseReferenceItem(node, location);
    }

    private ComputeUnit ParseReferenceItem(ParseTreeNode node, Location location)
    {
        Reference reference = Reference.Parse(node.Print());

        return reference switch
        {
            Location locationRef => new CellReference(location, locationRef with {Spreadsheet = locationRef.Spreadsheet ?? location.Spreadsheet}),
            Range range => new RangeReference(location, range with {To = range.To with {Spreadsheet = range.To.Spreadsheet ?? location.Spreadsheet}, From = range.From with {Spreadsheet = range.From.Spreadsheet ?? location.Spreadsheet}}),
            TableReference tableRef => new Domain.Compute.TableReference(location, tableRef),
            _ => throw new ArgumentException("Unsupported reference type.", nameof(reference))
        };
    }

    private ComputeUnit ParseFunctionCall(ParseTreeNode node, Location location)
    {
        if (node.IsBinaryOperation())
        {
            // Binary operator
            Function func = new Function(node.ChildNodes[1].Token.ValueString, location);
            func.AddDependency(ConvertParseTreeToFormula(node.ChildNodes[0], location));
            func.AddDependency(ConvertParseTreeToFormula(node.ChildNodes[2], location));
            return func;
        }
        
        if (node.IsFunction())
        {
            // Excel function call
            Function func = new Function(node.GetFunction(), location);
            foreach (var child in node.GetFunctionArguments().Select(c => ConvertParseTreeToFormula(c, location)))
            {
                func.AddDependency(child);
            }

            return func;
        }
        
        // It can also be a unary operator like a percentage or something
        if (node.IsUnaryPrefixOperation())
        {
            return new Function(node.ChildNodes[0].Token.ValueString + node.ChildNodes[1].Token.ValueString, location);
        }
        
        throw new ArgumentException("Unsupported parse tree node.", nameof(node));
    }
}