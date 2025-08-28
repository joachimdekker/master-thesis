using ExcelCompiler.Evaluation.Config;
using ExcelCompiler.Evaluation.Models;
using ExcelCompiler.Evaluation.Utils;
using OfficeOpenXml;

namespace ExcelCompiler.Evaluation.Services;

public sealed class TrialGenerator(Random rng)
{
    public Trial GenerateTrial(string spreadsheetUrl)
    {
        var workbook = new ExcelPackage(spreadsheetUrl).Workbook;
        
        var output = GenerateCellReference(workbook);
        var input  = GenerateCellReference(workbook);
        
        return new Trial(spreadsheetUrl, [input], [output]);
    }

    public CellReference GenerateCellReference(ExcelWorkbook workbook)
    {
        // Generate a random trial by selecting a random worksheet, random cell and random formula
        ExcelWorksheet worksheet = workbook.Worksheets.Random(rng);
        var cell = worksheet.Cells
            .Where(c => c.Value is not string and not null and not DateTime)
            .Random(rng);
        
        return new CellReference(worksheet.Name, cell.Address);
    }
}