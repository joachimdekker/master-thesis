using System.Diagnostics;
using ExcelCompiler.Domain.Compute;
using ExcelCompiler.Domain.Structure;
using Range = ExcelCompiler.Domain.Structure.Range;
using TableReference = ExcelCompiler.Domain.Compute.TableReference;

namespace ExcelCompiler.Passes;

public class LinkDependencies
{
    public SupportGraph Link(Workbook workbook, IEnumerable<Location> results)
    {
        // Get the cells of the roots
        List<ComputeUnit> rootUnits = [];
        HashSet<ComputeUnit> visited = new HashSet<ComputeUnit>();
        foreach (Location location in results)
        {
            Cell cell = workbook.GetCell(location);
            
            if (cell is FormulaCell fcell)
            {
                Link(fcell.ComputePlan, workbook, visited);
                rootUnits.Add(fcell.ComputePlan);
            }
        }
        
        return new SupportGraph
        {
            Roots = rootUnits,
        };
    }

    private void Link(ComputeUnit cell, Workbook workbook, HashSet<ComputeUnit> visited)
    {
        if (!visited.Add(cell))
        {
            return;
        }

        switch (cell)
        {
            case TableReference tableReference:
                Debug.Assert(tableReference.Dependencies.Count == 0, "Table should not have any dependencies yet.");
                
                // Get the table range from the workbook
                Range tableRange = GetTableRange(tableReference, workbook);
                
                // Replace the tableReference for a rangeReference
                RangeReference rangeReference = new RangeReference(tableReference.Location, tableRange);

                foreach (var dependent in tableReference.Dependents.ToList())
                {
                    dependent.RemoveDependency(tableReference);
                    dependent.AddDependency(rangeReference);
                }
                
                Link(rangeReference, workbook, visited);
                
                break;
            case CellReference reference:
                Debug.Assert(reference.Dependencies.Count == 0, "Cell should not have any dependencies yet.");
                
                var unit = GetUnit(location: reference.Reference, workbook);
                Link(unit, workbook, visited);
                reference.AddDependency(unit);
                
                break;
            case RangeReference range:
                Debug.Assert(range.Dependencies.Count == 0, "Range should not have any dependencies yet.");
                // We need to find a better way to do this.
                // Right now, we just go through everything and link everything.
                // However, if we traverse the twice and go to the range reference, that means that we traverse
                // The range twice and add double the dependencies, which is not good.
                // if (range.Dependencies.Count != 0)
                // {
                //     break;
                // }
                // Okay, so doing it with a visited HashSet seems to be the better way to do this.
                // However, it seems that we need to always do this, so perhaps we need to have a better way to traverse the tree
                // Perhaps we need to make sure that we always traverse the tree in a topological order.
                // Good notes.
                
                // We consider every cell in the range to be a dependency of the range
                // This will probably bite us in the butt in the future, for example with SUMIF logic
                // If I think about it now, we could probably just add the range as a dependency, and then
                // The cells in that range as a dependency of the range. Problem solved.
                foreach (var location in range.Reference.GetLocations())
                {
                    var rangeUnit = GetUnit(location: location, workbook);
                    Link(rangeUnit, workbook, visited);
                    range.AddDependency(rangeUnit);
                }

                break;
                // Do something with range, we don't do anything with this now.
            default:
                foreach (ComputeUnit dependencies in cell.Dependencies.ToList())
                {
                    Link(dependencies, workbook, visited);
                }
                break;
        }
    }

    private ComputeUnit GetUnit(Location location, Workbook workbook)
    {
        // Get the spreadsheet
        string? spreadsheet = location.Spreadsheet;
        if (location.Spreadsheet is null)
        {
            throw new ArgumentException("Location does not have a spreadsheet.", nameof(location));
        }
        
        // Get the cell
        Cell? cell = workbook.GetCell(location);

        return cell switch
        {
            FormulaCell formula => formula.ComputePlan,
            ValueCell<string> stringValue => new ConstantValue<string>(stringValue.Value, location),
            ValueCell<double> doubleValue => new ConstantValue<double>(doubleValue.Value, location),
            ValueCell<decimal> decimalValue => new ConstantValue<decimal>(decimalValue.Value, location),
            ValueCell<bool> boolValue => new ConstantValue<bool>(boolValue.Value, location),
            ValueCell<DateTime> dateTimeValue => new ConstantValue<DateTime>(dateTimeValue.Value, location),
            EmptyCell emptyCell => new Nil(location),
            _ => throw new ArgumentException("Unsupported cell type.", nameof(cell))
        };
    }

    private Range GetTableRange(TableReference tableReference, Workbook workbook)
    {
        foreach (Spreadsheet sheet in workbook.Spreadsheets)
        {
            foreach (Table table in sheet.Tables)
            {
                if (table.Name == tableReference.Reference.TableName)
                {
                    return table.Columns[tableReference.Reference.ColumnName];
                }
            }
        }
        
        throw new ArgumentException("Table not found.", nameof(tableReference));
    }

}