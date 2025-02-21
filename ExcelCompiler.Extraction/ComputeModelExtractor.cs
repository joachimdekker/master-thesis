using System.Globalization;
using System.Text.RegularExpressions;
using ExcelCompiler.Domain;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Spreadsheet;
using Irony.Parsing;
using OfficeOpenXml;
using XLParser;

namespace ExcelCompiler.Extraction;

public partial class ComputeModelExtractor
{
    public ComputeModelExtractor()
    {
    }
    
    [GeneratedRegex("[A-Z]+[1-9][0-9]*")]
    static partial Regex A1FormatRegex { get; }

    public List<Cell> Extract(Stream excelFile)
    {
        using var p = new ExcelPackage(excelFile);

        List<Cell> cells = new();
        Location startCell = Location.FromA1Format("F17");

        Queue<Location> cellsToProcess = new([startCell]);
        HashSet<Location> processedCells = new();

        while (cellsToProcess.Count > 0)
        {
            var cellLocation = cellsToProcess.Dequeue();
            var cell = p.Workbook.Worksheets[cellLocation.WorksheetIndex].Cells[cellLocation.Row, cellLocation.Column];
            
            // Check if the cell contains a value or a formula
            bool isFormula = cell.Formula is not null and not "";

            if (!isFormula && cell.Value is null or "") continue;
            
            processedCells.Add(cellLocation);
            if (!isFormula)
            {
                Cell valueCell = new ValueCell(cellLocation, cell.Value.ToString()!);

                cells.Add(valueCell);
                continue;
            }

            ParseTreeNode node = ExcelFormulaParser.Parse(cell.Formula!);

            // Get every cell that is mentioned in the formula
            var matches = A1FormatRegex.Matches(cell.Formula!);

            foreach (Match match in matches)
            {
                var matchString = match.Value;
                var location = Location.FromA1Format(matchString);

                if (processedCells.Contains(location)) continue;

                cellsToProcess.Enqueue(location);
            }

            // Convert the parse tree node to a formula
            Function formula = ConvertParseTreeToFormula(node);

            FormulaCell cellToAdd = new(cellLocation, formula);
            cells.Add(cellToAdd);
        }
        
        return cells;
    }

    private Function ConvertParseTreeToFormula(ParseTreeNode node)
    {
        if (node.IsParentheses()) return ConvertParseTreeToFormula(node.ChildNodes[1]);
        
        
        
        return node.Type() switch
        {
            GrammarNames.Formula => ConvertParseTreeToFormula(node.ChildNodes[0]),
            GrammarNames.FunctionCall => ParseFunctionCall(node),
            GrammarNames.Text => new ConstantValue<string>(node.Token.ValueString),
            GrammarNames.Number => new ConstantValue<double>(double.Parse(node.Token.ValueString)),
            GrammarNames.Reference => ParseReference(node),
            GrammarNames.UDFunctionCall => throw new ArgumentException(
                "User-defined functions are not supported at this time.", nameof(node)),
            _ => node switch
            {
                { ChildNodes.Count: 1 } childNode => ConvertParseTreeToFormula(childNode),
                _ => throw new ArgumentException("Unsupported parse tree node.", nameof(node))
            }
        };
    }


    private Function ParseReference(ParseTreeNode node)
    {
        if (node.IsRange())
        {
            return new Function("Range", [
                ParseReferenceItem(node.ChildNodes[0]),
                ParseReferenceItem(node.ChildNodes[2])
            ]);
        }
        
        return ParseReferenceItem(node.ChildNodes[0]);
    }

    private Function ParseReferenceItem(ParseTreeNode node) => node.Type() switch
    {
        GrammarNames.Cell => new Reference(node.ChildNodes[0].Token.ValueString),
        GrammarNames.NamedRange => new Reference("NamedRange:" + node.ChildNodes[0].Token.ValueString),
        GrammarNames.HorizontalRange => new Reference("HorizontalRange:" + node.ChildNodes[0].Token.ValueString),
        GrammarNames.VerticalRange => new Reference("VerticalRange:" + node.ChildNodes[0].Token.ValueString),
        GrammarNames.RefError => throw new ArgumentException(
            "References to error values are not supported at this time"),
        GrammarNames.StructuredReference => throw new ArgumentException(
            "Structured References are not supported at this time")
    };

    private Function ParseFunctionCall(ParseTreeNode node)
    {
        if (node.IsBinaryOperation())
        {
            // Binary operator
            return new Function(node.ChildNodes[1].Token.ValueString, [
                ConvertParseTreeToFormula(node.ChildNodes[0]),
                ConvertParseTreeToFormula(node.ChildNodes[2])
            ]);
        }
        
        if (node.ChildNodes[0].Type() == GrammarNames.FunctionName)
        {
            // Excel function call
            return new Function(node.ChildNodes[0].Token.ValueString,
                node.ChildNodes[1..].Select(ConvertParseTreeToFormula).ToList());
        }
        
        // It can also be a unary operator like a percentage or something

        if (node.IsUnaryPrefixOperation())
        {
            return new Function(node.ChildNodes[0].Token.ValueString + node.ChildNodes[1].Token.ValueString, []);
        }

        if (node.IsUnaryPostfixOperation())
        {
            // There is only one unary postfix operation: '%', and thus we convert the value to decimal
            double value = double.Parse(node.ChildNodes[0].Token.ValueString);
            return new Function(value.ToString(CultureInfo.InvariantCulture), []);
        }
        
        throw new ArgumentException("Unsupported parse tree node.", nameof(node));
    }
}