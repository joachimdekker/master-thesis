using ExcelCompiler.Representations.Structure;

namespace ExcelCompiler.Passes.Structure;

[CompilerPass]
public class DetectStructures(DetectTables tableDetector, DetectChains chainDetector, DetectDataPond dataPondDetector)
{
    public List<Construct> Detect(Workbook workbook, List<Area> areas)
    {
        return areas
            .Select(a => Detect(workbook, a))
            .Where(a => a is not null)
            .Cast<Construct>()
            .ToList();
    }

    private Construct? Detect(Workbook workbook, Area area)
    {
        // Get the spreadsheet
        Spreadsheet spreadsheet = workbook.Spreadsheets.First(s => s.Name == area.Range.Spreadsheet);
        
        // Test if the area is a table
        // TODO: Add support for table footers
        if (tableDetector.IsTable(spreadsheet, area))
        {
            return tableDetector.Convert(spreadsheet, area);
        }
        
        // Test if the area is a chain
        if (chainDetector.IsChain(spreadsheet, area))
        {
            return chainDetector.Convert(spreadsheet, area);
        }

        // Test if the area is a data pond
        // if (dataPondDetector.IsDataPond(spreadsheet, area))
        // {
        //     return dataPondDetector.Convert(spreadsheet, area);
        // }
        
        return null!;
    }
}