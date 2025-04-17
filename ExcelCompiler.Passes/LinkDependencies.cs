using System.Diagnostics;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Structure;
using Range = ExcelCompiler.Domain.Structure.Range;

namespace ExcelCompiler.Passes;

public class LinkDependencies
{
    public SupportGraph Link(Workbook workbook, IEnumerable<Location> results)
    {
        // First link everything
        // Find all References and Ranges in the units and add dependencies.
        
        
        // Get the cells of the roots
        List<ComputeUnit> rootUnits = [];
        foreach (Spreadsheet sheet in workbook.Spreadsheets)
        {
            foreach (FormulaCell cell in sheet.Cells.Where(cell => results.Contains(cell.Location) && cell is FormulaCell))
            {
                Link(cell.ComputePlan, workbook);
                rootUnits.Add(cell.ComputePlan);
            }
        }
        
        return new SupportGraph
        {
            Roots = rootUnits,
        };
    }

    private void Link(ComputeUnit cell, Workbook workbook)
    {
        switch (cell)
        {
            case CellReference reference:
                Debug.Assert(reference.Dependencies.Count == 0);
                var unit = GetUnit(location: reference.Reference);
                reference.AddDependency(unit);
                break;
            case RangeReference range:
                Debug.Assert(range.Dependencies.Count == 0);
                
                // We consider every cell in the range to be a dependency of the range
                // This will probably bite us in the butt in the future, for example with SUMIF logic
                // If I think about it now, we could probably just add the range as a dependency, and then
                // The cells in that range as a dependency of the range. Problem solved.
                foreach (var location in range.Reference.GetLocations())
                {
                    var rangeUnit = GetUnit(location: location);
                    range.AddDependency(rangeUnit);
                }

                break;
                // Do something with range, we don't do anything with this now.
            default:
                foreach (ComputeUnit dependencies in cell.Dependencies.ToList())
                {
                    Link(dependencies, workbook);
                }
                break;
        }
    }

    private ComputeUnit GetUnit(Location location)
    {
        // Get the spreadsheet
        Spreadsheet? spreadsheet = location.Spreadsheet;
        if (spreadsheet is null)
        {
            throw new ArgumentException("Location does not have a spreadsheet.", nameof(location));
        }
        
        // Get the cell
        Cell? cell = spreadsheet[location];

        return cell switch
        {
            FormulaCell formula => formula.ComputePlan,
            ValueCell<string> stringValue => new ConstantValue<string>(stringValue.Value, location),
            ValueCell<double> doubleValue => new ConstantValue<double>(doubleValue.Value, location),
            ValueCell<decimal> decimalValue => new ConstantValue<decimal>(decimalValue.Value, location),
            ValueCell<bool> boolValue => new ConstantValue<bool>(boolValue.Value, location),
            ValueCell<DateTime> dateTimeValue => new ConstantValue<DateTime>(dateTimeValue.Value, location),
            _ => throw new ArgumentException("Unsupported cell type.", nameof(cell))
        };
    }
}