using ExcelCompiler.Representations.Structure;
using Microsoft.Extensions.Logging;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Passes.Structure;

[CompilerPass]
public class DetectStructures(DetectTables tableDetector, DetectChains chainDetector, DetectDataPond dataPondDetector, ILogger<DetectStructures> logger)
{
    
    public List<Construct> Detect(Workbook workbook, List<Area> areas, List<Range> inputs)
    {
        var constructs = areas
            .Select(a => Detect(workbook, a))
            .Where(a => a is not null)
            .Cast<Construct>()
            .ToList();

        foreach (var construct in constructs)
        {
            Range? input;
            if ((input = inputs.SingleOrDefault(i => construct.Location.Contains(i))) is null) continue;
            if (!input.Equals(construct.Location)) logger.LogWarning("Structure is not fully covered");
            construct.IsInput = true;
        }

        return constructs;
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