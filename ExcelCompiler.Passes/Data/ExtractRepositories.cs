using System.Data;
using ExcelCompiler.Representations.Data;
using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Passes.Data;

[CompilerPass]
public class ExtractRepositories
{
    public List<IDataRepository> Transform(Workbook workbook)
    {
        // Convert all tables to data repositories
        IEnumerable<IDataRepository> repositories =
            from table in workbook.Constructs.OfType<Table>()
            let properties = GetTableProperties(workbook, table)
            let scheme = new ColumnarDataSchema
            {
                Name = table.Name, 
                Properties = properties
            }
            let data = ExtractDataFromSheet(workbook, table, scheme)
            select new InMemoryDataRepository(table.Name, scheme, data);
        
        return repositories.ToList();
    }

    private object[,] ExtractDataFromSheet(Workbook workbook, Chain chain, ColumnarDataSchema scheme)
    {
        throw new NotImplementedException();
    }

    private List<KeyValuePair<string, Type>> GetChainProperties(Workbook workbook, Chain chain)
    {
        Dictionary<string, Type> properties = new Dictionary<string, Type>();
        foreach (var (columnName, columnRange) in chain.Columns)
        {
            var cell = GetFirstNonEmptyCell(workbook, columnRange.Range);

            if (cell is null or FormulaCell) continue;
            
            properties[columnName] = cell.Type;
        }
        
        return properties.ToList();
    }

    private List<KeyValuePair<string, Type>> GetTableProperties(Workbook workbook, Table table)
    {
        Dictionary<string, Type> properties = new Dictionary<string, Type>();
        foreach (var (columnName, columnRange) in table.Columns)
        {
            var cell = GetFirstNonEmptyCell(workbook, columnRange.Range);

            if (cell is null or FormulaCell) continue;
            
            properties[columnName] = cell.Type;
        }
        
        return properties.ToList();
    }

    private object[,] ExtractDataFromSheet(Workbook workbook, Table table, ColumnarDataSchema scheme)
    {
        // Get all values in the table
        
        // Get the column values
        List<List<object>> columns = [];
        foreach (var (columnName, columnType) in scheme.Properties)
        {
            LineSelection columnRange = table.Columns[columnName];
            
            // For every location, get the value
            List<object> columnValues = [];
            foreach (var location in columnRange.Range)
            {
                var cell = workbook[location];
                
                var value = cell switch
                {
                    EmptyCell => columnType.GetDefaultValue(),
                    ValueCell vcell => vcell.Value,
                    _ => throw new InvalidOperationException("Unsupported cell type.")
                };
                
                columnValues.Add(value);
            }
            
            columns.Add(columnValues);
        }
        
        // Transpose the columns
        object[,] data = (columns as IEnumerable<IEnumerable<object>>).Transpose();
        
        return data;
    }

    private Cell? GetFirstNonEmptyCell(Workbook workbook, Range range)
    {
        foreach (var cell in range)
        {
            if (workbook[cell] is EmptyCell) continue;
            
            return workbook[cell];
        }

        return null!;
    }
}