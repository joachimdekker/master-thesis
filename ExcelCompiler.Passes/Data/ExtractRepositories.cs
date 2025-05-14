using ExcelCompiler.Representations.Data;
using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.Structure;
using Range = ExcelCompiler.Representations.Structure.Range;

namespace ExcelCompiler.Passes.Data;

[CompilerPass]
public class ExtractRepositories
{
    public List<IDataRepository> Transform(Workbook workbook)
    {
        // Convert all tables to data repositories
        IEnumerable<IDataRepository> repositories =
            from table in workbook.Tables
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

    private List<KeyValuePair<string, Type>> GetTableProperties(Workbook workbook, Table table)
    {
        Dictionary<string, Type> properties = new Dictionary<string, Type>();
        foreach (var (columnName, columnRange) in table.Columns)
        {
            var cell = GetFirstNonEmptyCell(workbook, columnRange);

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
            Range columnRange = table.Columns[columnName];
            
            // For every location, get the value
            List<object> columnValues = [];
            foreach (var location in columnRange.GetLocations())
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
        object[,] data = new object[columns[0].Count, columns.Count];
        for (int i = 0; i < columns.Count; i++)
        {
            for (int j = 0; j < columns[0].Count; j++)
            {
                data[j, i] = columns[i][j];
            }
        }
        
        return data;
    }

    private Cell? GetFirstNonEmptyCell(Workbook workbook, Range range)
    {
        foreach (var cell in range.GetLocations())
        {
            if (workbook[cell] is EmptyCell) continue;
            
            return workbook[cell];
        }

        return null!;
    }
}