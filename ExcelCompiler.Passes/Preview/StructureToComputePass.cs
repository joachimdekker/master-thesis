using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using ExcelCompiler.Representations.Structure.Formulas;
using CellReference = ExcelCompiler.Representations.Compute.CellReference;
using Function = ExcelCompiler.Representations.Compute.Function;
using RangeReference = ExcelCompiler.Representations.Compute.RangeReference;

namespace ExcelCompiler.Passes.Preview;

[CompilerPass]
public class StructureToComputePass
{
    public ComputeGrid Transform(Workbook workbook, List<Location> results)
    {
        ComputeGrid computeGrid = new ComputeGrid();
        PopulateComputeGrid(workbook, results, computeGrid);
        
        return computeGrid;
    }

    private void PopulateComputeGrid(Workbook workbook, IEnumerable<Location> results, ComputeGrid computeGrid)
    {
        Stack<Location> stack = new Stack<Location>(results);
        while(stack.Count != 0) {
            Location location = stack.Pop();
            Cell cell = workbook.GetCell(location);
        
            var computeUnit = CreateComputeUnitFromCell(location, cell);

            computeGrid[location] = computeUnit;

            var references = GetReferences(computeUnit, workbook)
                .Where(r => !computeGrid.ContainsLocation(r))
                .Distinct();
            
            stack.PushRange(references);
        }
    }

    private static ComputeUnit CreateComputeUnitFromCell(Location location, Cell cell)
    {
        FormulaConverter formulaConverter = new(location);
        ComputeUnit computeUnit = cell switch
        {
            FormulaCell formulaCell => formulaConverter.Traverse(formulaCell.Formula),
            ValueCell<string> stringValue => new ConstantValue<string>(stringValue.Value, location),
            ValueCell<double> doubleValue => new ConstantValue<double>(doubleValue.Value, location),
            ValueCell<decimal> decimalValue => new ConstantValue<decimal>(decimalValue.Value, location),
            ValueCell<bool> boolValue => new ConstantValue<bool>(boolValue.Value, location),
            ValueCell<DateTime> dateTimeValue => new ConstantValue<DateTime>(dateTimeValue.Value, location),
            EmptyCell => new Nil(location),
            _ => throw new ArgumentException("Unsupported cell type.", nameof(cell))
        };
        return computeUnit;
    }

    private IEnumerable<Location> GetReferences(ComputeUnit formula, Workbook workbook)
    {
        return formula switch
        {
            CellReference cellReference => [cellReference.Reference],
            RangeReference rangeReference => rangeReference.Reference,
            Representations.Compute.TableReference tableReference => workbook.Constructs.OfType<Table>()
                .Single(t => t.Name == tableReference.Reference.TableName)
                .Columns[tableReference.Reference.ColumnNames[0]].Range,
            Function function => function.Dependencies
                .SelectMany(f => GetReferences(f, workbook)),
            _ => [],
        };
    }
}

file record FormulaConverter(Location Location) : FormulaTraverser<ComputeUnit>
{
    protected override ComputeUnit Constant(Constant constant) =>
        constant.Value switch
        {
            string s => new ConstantValue<string>(s, Location),
            double d => new ConstantValue<double>(d, Location),
            decimal d => new ConstantValue<decimal>(d, Location),
            bool b => new ConstantValue<bool>(b, Location),
            DateTime dt => new ConstantValue<DateTime>(dt, Location),
            _ => throw new ArgumentException("Unsupported constant type.", nameof(constant))
        };

    protected override ComputeUnit Function(Representations.Structure.Formulas.Function function, List<ComputeUnit> arguments) => new Function(Location, function.Name, arguments);

    protected override ComputeUnit Operator(Operator @operator, List<ComputeUnit> arguments) => new Function(Location, @operator.Name, arguments);

    protected override ComputeUnit CellReference(Representations.Structure.Formulas.CellReference reference) => new CellReference(Location, reference.Reference);

    protected override ComputeUnit RangeReference(Representations.Structure.Formulas.RangeReference reference) => new RangeReference(Location, reference.Reference);

    protected override ComputeUnit TableReference(Representations.Structure.Formulas.TableReference reference) => new Representations.Compute.TableReference(Location, reference.Reference);
}