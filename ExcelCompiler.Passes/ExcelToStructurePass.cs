using ExcelCompiler.Representations.References;
using ExcelCompiler.Representations.Structure;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using ExcelTable = ExcelCompiler.Representations.Structure.ExcelTable;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Passes;


[CompilerPass]
public class ExcelToStructurePass
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
            foreach (var cellPosition in sheet.Dimension)
            {
                if (cellPosition is null || (string.IsNullOrEmpty(cellPosition.Formula) && string.IsNullOrEmpty(cellPosition.Value?.ToString()))) continue;
                
                // Convert the cell.
                Location location = Location.FromA1(cellPosition.Address, spreadsheet.Name);
                Cell? cell = GetCell(location, p);
                
                if (cell is not null)
                {
                    spreadsheet[location] = cell;
                }
            }
            
            
            // // Get all the tables in the excel file
            foreach (OfficeOpenXml.Table.ExcelTable excelTable in sheet.Tables)
            {
                Range tableRange = (Reference.Parse(excelTable.Range.Address) as Range)!;
                // Temp to fix the spreadsheet linkage problem.
                tableRange.From.Spreadsheet = spreadsheet.Name;
                tableRange.To.Spreadsheet = spreadsheet.Name;
                
                ExcelTable table = new ExcelTable
                {
                    Name = excelTable.Name,
                    Location = tableRange,
                };
                
                spreadsheet.Tables.Add(table);
            }
            
            workbook.Spreadsheets.Add(spreadsheet);
        }

        return workbook;
    }

    private Cell? GetCell(Location cellLocation, ExcelPackage excelFile)
    {
        var cell = excelFile.Workbook.Worksheets[cellLocation.Spreadsheet].Cells[cellLocation.Row, cellLocation.Column];
        
        // If the cell is a merged cell, only return when we target the left-most cell
        // if (cell.Merge) 
        
        // Check if the cell contains a value or a formula
        bool isFormula = cell.Formula is not null and not "";
        if (!isFormula && cell.Value is null or "") return null!;
            
        if (!isFormula)
        {
            Cell valueCell = cell.Value switch
            {
                string str => new ValueCell<string>(str, cellLocation),
                double d => new ValueCell<double>(d, cellLocation),
                decimal d => new ValueCell<decimal>(d, cellLocation),
                bool b => new ValueCell<bool>(b, cellLocation),
                DateTime dt => new ValueCell<DateTime>(dt, cellLocation),
                _ => throw new ArgumentException("Unsupported cell value type.", nameof(cell))
            };
            
            return valueCell;
        }
        
        return new FormulaCell(cellLocation, cell.Value.GetType(), cell.Formula!);
    }
}