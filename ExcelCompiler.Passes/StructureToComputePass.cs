using ExcelCompiler.Domain.Structure;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Structure;
using Irony.Parsing;
using XLParser;
using Range = ExcelCompiler.Representations.Structure.Range;
using TableReference = ExcelCompiler.Representations.Structure.TableReference;

namespace ExcelCompiler.Passes;

[CompilerPass]
public class StructureToComputePass
{
    public Dictionary<Location, ComputeUnit> Transform(Workbook workbook, IEnumerable<Location> results)
    {
        Dictionary<Location, ComputeUnit> resultsDictionary = new Dictionary<Location, ComputeUnit>();
        
        Stack<Location> stack = new Stack<Location>(results);
        while(stack.Count != 0) {
            Location location = stack.Pop();
            Cell cell = workbook.GetCell(location);

            ComputeUnit computeUnit = cell switch
            {
                FormulaCell formulaCell => Parse(formulaCell.Raw, location),
                ValueCell<string> stringValue => new ConstantValue<string>(stringValue.Value, location),
                ValueCell<double> doubleValue => new ConstantValue<double>(doubleValue.Value, location),
                ValueCell<decimal> decimalValue => new ConstantValue<decimal>(decimalValue.Value, location),
                ValueCell<bool> boolValue => new ConstantValue<bool>(boolValue.Value, location),
                ValueCell<DateTime> dateTimeValue => new ConstantValue<DateTime>(dateTimeValue.Value, location),
                EmptyCell => new Nil(location),
                _ => throw new ArgumentException("Unsupported cell type.", nameof(cell))
            };
            
            resultsDictionary[location] = computeUnit;

            foreach (Location reference in GetReferences(computeUnit, workbook))
            {
                if (resultsDictionary.ContainsKey(reference))
                {
                    continue;
                }
                
                stack.Push(reference);
            }
        }
        
        return resultsDictionary;
    }

    private IEnumerable<Location> GetReferences(ComputeUnit formula, Workbook workbook)
    {
        return formula switch
        {
            CellReference cellReference => [cellReference.Reference],
            RangeReference rangeReference => rangeReference.Reference.GetLocations(),
            Representations.Compute.TableReference tableReference => workbook.Tables
                .Single(t => t.Name == tableReference.Reference.TableName)
                .Columns[tableReference.Reference.ColumnName]
                .GetLocations(),
            Function function => function.Dependencies
                .SelectMany(f => GetReferences(f, workbook)),
            _ => [],
        };
    }

    private ComputeUnit Parse(string formula, Location location)
    {
        ParseTreeNode root = ExcelFormulaParser.Parse(formula);
        
        return Parse(root, location);
    }

    private ComputeUnit Parse(ParseTreeNode root, Location location)
    {
        ParseTreeNode node = root.SkipToRelevant();

        return node.Type() switch
        {
            GrammarNames.FunctionCall => ParseFunctionCall(node, location),
            GrammarNames.Reference => ParseReference(node, location),
            GrammarNames.Constant => Parse(node.ChildNodes[0], location),
            GrammarNames.Text => new ConstantValue<string>(node.ChildNodes[0].Token.ValueString, location),
            GrammarNames.Number => new ConstantValue<double>(double.Parse(node.ChildNodes[0].Token.ValueString), location),
            _ => throw new InvalidOperationException("Node type is not supported.")
        };
    }
    
    private ComputeUnit ParseReference(ParseTreeNode node, Location location)
    {
        Reference reference = Reference.Parse(node.Print(), spreadsheet: location.Spreadsheet);

        return reference switch
        {
            Location locationRef => new CellReference(location, locationRef),
            Range range => new RangeReference(location, range),
            TableReference tableRef => new Representations.Compute.TableReference(location, tableRef),
            _ => throw new ArgumentException("Unsupported reference type.", nameof(reference))
        };
    }
    
    private ComputeUnit ParseFunctionCall(ParseTreeNode node, Location location)
    {
        if (node.IsBinaryOperation())
        {
            // Binary operator
            Function func = new Function(location, node.ChildNodes[1].Token.ValueString);
            func.AddDependency(Parse(node.ChildNodes[0], location));
            func.AddDependency(Parse(node.ChildNodes[2], location));
            return func;
        }
        
        if (node.IsFunction())
        {
            // Excel function call
            Function func = new Function(location, node.GetFunction());
            IEnumerable<ComputeUnit> dependencies = node.GetFunctionArguments().Select(c => Parse(c, location));
            func.AddDependencies(dependencies);

            return func;
        }
        
        // It can also be a unary operator like a percentage or something
        if (node.IsUnaryPrefixOperation())
        {
            return new Function(location, node.ChildNodes[0].Token.ValueString + node.ChildNodes[1].Token.ValueString);
        }
        
        throw new ArgumentException("Unsupported parse tree node.", nameof(node));
    }

}