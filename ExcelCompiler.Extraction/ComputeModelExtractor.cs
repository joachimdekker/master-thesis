using System.Globalization;
using System.Text.RegularExpressions;
using ExcelCompiler.Domain;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Spreadsheet;
using Irony.Parsing;
using OfficeOpenXml;
using XLParser;
using Range = ExcelCompiler.Domain.Compute.Range;

namespace ExcelCompiler.Extraction;

public partial class ComputeModelExtractor
{
    public ComputeModelExtractor()
    {
    }
    
    [GeneratedRegex("[A-Z]+[1-9][0-9]*")]
    static partial Regex A1FormatRegex { get; }

    public SupportGraph Extract(Stream excelFile, Location resultLocation)
    {
        using var p = new ExcelPackage(excelFile);

        List<Cell> cells = [];
        
        Location startCell = resultLocation;
        SupportGraph graph = new SupportGraph();

        Dictionary<Location, ComputeUnit> processedCells = [];
        

        ComputeUnit? root = GetComputeUnit(startCell, p);
        
        if (root is null) return graph;
        
        // Add the root to the graph
        graph.Roots.Add(root);
        
        // Prepare the traversal algorithm
        // We use a stack for DFS to make linking the graph easier
        List<Location> locationToTraverse = GetNextLocations(root);
        Stack<(Location Location, ComputeUnit Parent)> cellsToProcess = new(locationToTraverse.Select(l => (l, root)));
        
        
        while (cellsToProcess.Count > 0)
        {
            (Location cellLocation, ComputeUnit parent) = cellsToProcess.Pop();
            var cu = GetComputeUnit(cellLocation, p);

            if (cu is null)
            {
                continue;
            }
            
            // Get the next locations
            List<Location> nextLocations = GetNextLocations(cu);
            foreach (var nextLocation in nextLocations)
            {
                if (processedCells.TryGetValue(nextLocation, out var nextCu))
                {
                    // If the next location is already processed, add a dependency to the current unit
                    cu.AddDependency(nextCu);
                    continue;
                }
                 
                cellsToProcess.Push((nextLocation, cu));
            }
            
            // Add the formula to the graph
            processedCells[cellLocation] = cu;
            parent.AddDependency(cu);
        }
        
        return graph;
    }

    private List<Location> GetNextLocations(ComputeUnit root)
    {
        return root switch 
        {
            FunctionComposition function => function.Arguments.SelectMany(GetNextLocations).ToList(),
            Reference reference => [reference.CellReference],
            Range range => range.GetLocations().ToList(),
            _ => [],
        };
    }

    private ComputeUnit? GetComputeUnit(Location cellLocation, ExcelPackage excelFile)
    {
        var cell = excelFile.Workbook.Worksheets[cellLocation.WorksheetIndex].Cells[cellLocation.Row, cellLocation.Column];
            
        // Check if the cell contains a value or a formula
        bool isFormula = cell.Formula is not null and not "";
        if (!isFormula && cell.Value is null or "") return null!;
            
        if (!isFormula)
        {
            ComputeUnit valueCell = new ConstantValue<string>(cell.Value.ToString()!, cellLocation);
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
        if (node.IsRange())
        {
            return new FunctionComposition("Range", [
                ParseReferenceItem(node.ChildNodes[0], location),
                ParseReferenceItem(node.ChildNodes[2], location)
            ], location);
        }
        
        return ParseReferenceItem(node.ChildNodes[0], location);
    }

    private ComputeUnit ParseReferenceItem(ParseTreeNode node, Location location) => node.Type() switch
    {
        GrammarNames.Cell => new Reference(Location.FromA1(node.ChildNodes[0].Token.ValueString), location),
        GrammarNames.NamedRange => Range.FromString(node.ChildNodes[0].Token.ValueString, location),
        // GrammarNames.HorizontalRange => Range.FromString(node.ChildNodes[0].Token.ValueString, location),
        // GrammarNames.VerticalRange => Range.FromString(node.ChildNodes[0].Token.ValueString, location),
        GrammarNames.RefError => throw new ArgumentException(
            "References to error values are not supported at this time"),
        GrammarNames.StructuredReference => throw new ArgumentException(
            "Structured References are not supported at this time"),
        _ => throw new ArgumentOutOfRangeException("This is not supported at the time.")
    };

    private ComputeUnit ParseFunctionCall(ParseTreeNode node, Location location)
    {
        if (node.IsBinaryOperation())
        {
            // Binary operator
            return new FunctionComposition(node.ChildNodes[1].Token.ValueString, [
                ConvertParseTreeToFormula(node.ChildNodes[0], location),
                ConvertParseTreeToFormula(node.ChildNodes[2], location)
            ], location);
        }
        
        if (node.ChildNodes[0].Type() == GrammarNames.FunctionName)
        {
            // Excel function call
            return new FunctionComposition(node.ChildNodes[0].Token.ValueString,
                node.ChildNodes[1..].Select(c => ConvertParseTreeToFormula(c, location)).ToList(), location);
        }
        
        // It can also be a unary operator like a percentage or something

        if (node.IsUnaryPrefixOperation())
        {
            return new FunctionComposition(node.ChildNodes[0].Token.ValueString + node.ChildNodes[1].Token.ValueString, [], location);
        }

        if (node.IsUnaryPostfixOperation())
        {
            // There is only one unary postfix operation: '%', and thus we convert the value to decimal
            double value = double.Parse(node.ChildNodes[0].Token.ValueString);
            return new FunctionComposition(value.ToString(CultureInfo.InvariantCulture), [], location);
        }
        
        throw new ArgumentException("Unsupported parse tree node.", nameof(node));
    }
}