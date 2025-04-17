using System.Globalization;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Structure;
using Irony.Parsing;
using OfficeOpenXml;
using XLParser;
using Range = ExcelCompiler.Domain.Structure.Range;

namespace ExcelCompiler.Passes;

public class FrontendPass
{
    private Dictionary<string, Reference> _namedRanges = [];
    
    public Workbook Transform(Stream excelFile)
    {
        using var p = new ExcelPackage(excelFile);
        
        // Convert the named ranges
        _namedRanges = p.Workbook.Names
            .ToDictionary(namedRange => namedRange.Name, namedRange => Reference.Parse(namedRange.Address));
        
        Workbook workbook = new Workbook(name: p.File.Name)
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
                Location location = Location.FromA1(cell.Address, spreadsheet);
                ComputeUnit? computeUnit = GetComputeUnit(location, p);
                Cell? newCell = computeUnit switch
                {
                    ConstantValue<string> @string => new ValueCell<string>(location, @string.Value),
                    Function f => new FormulaCell(location, f),
                    _ => null,
                };
                
                if (newCell is not null)
                {
                    spreadsheet[location] = newCell;
                }
            }
            
            workbook.Spreadsheets.Add(spreadsheet);
        }

        return workbook;
    }

    private ComputeUnit? GetComputeUnit(Location cellLocation, ExcelPackage excelFile)
    {
        var cell = excelFile.Workbook.Worksheets[cellLocation.Spreadsheet?.Name].Cells[cellLocation.Row, cellLocation.Column];
            
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
            //GrammarNames.Reference => ParseReference(node, location),
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
        
        return ParseReferenceItem(node.ChildNodes[0], location);
    }

    private ComputeUnit ParseReferenceItem(ParseTreeNode node, Location location) => node.Type() switch
    {
        GrammarNames.Cell => new CellReference(Location.FromA1(node.ChildNodes[0].Token.ValueString), location),
        GrammarNames.NamedRange => _namedRanges[node.ChildNodes[0].Token.ValueString] switch
        {
            Location loc => new CellReference(location, loc),
            Range range => new RangeReference(location, range),
            _ => throw new ArgumentOutOfRangeException()
        },
        // GrammarNames.HorizontalRange => Range.FromString(node.ChildNodes[0].Token.ValueString, location),
        // GrammarNames.VerticalRange => Range.FromString(node.ChildNodes[0].Token.ValueString, location),
        GrammarNames.RefError => throw new ArgumentException(
            "References to error values are not supported at this time"),
        GrammarNames.StructuredReference => throw new ArgumentException(
            "Structured References are not supported at this time"),
        GrammarNames.ReferenceFunctionCall => new RangeReference(location, Range.FromString(node.Print())),
        _ => throw new ArgumentOutOfRangeException("This is not supported at the time.")
    };

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
        }
        
        // It can also be a unary operator like a percentage or something
        if (node.IsUnaryPrefixOperation())
        {
            return new Function(node.ChildNodes[0].Token.ValueString + node.ChildNodes[1].Token.ValueString, location);
        }
        
        throw new ArgumentException("Unsupported parse tree node.", nameof(node));
    }
}