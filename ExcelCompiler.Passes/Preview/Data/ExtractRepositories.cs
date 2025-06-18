using ExcelCompiler.Representations.Data.Preview;
using ExcelCompiler.Representations.Data.Preview.Specialized;
using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.Structure;
using IDataRepository = ExcelCompiler.Representations.Data.Preview.IDataRepository;
using InMemoryDataRepository = ExcelCompiler.Representations.Data.Preview.InMemoryDataRepository;

namespace ExcelCompiler.Passes.Preview.Data;

[CompilerPass]
public class ExtractRepositories
{
    public DataManager Transform(Workbook workbook)
    {
        // Convert all tables to data repositories
        IEnumerable<IDataRepository> repositories =
            from table in workbook.Constructs
            let schema = GetSchema(table)
            let repository = ExtractData(table, schema)
            select repository;

        return new DataManager(repositories);
    }

    private InMemoryDataRepository ExtractData(Construct construct, DataSchema schema) => construct switch
    {
        Table t => ExtractData(t, schema),
        Chain c => ExtractData(c, schema),
        _ => throw new InvalidOperationException("Unsupported construct type.")
    };

    private DataSchema GetSchema(Construct construct) => construct switch
    {
        Table t => GetSchema(t),
        Chain c => GetSchema(c),
        _ => throw new InvalidOperationException("Unsupported construct type.")
    };

    private InMemoryDataRepository ExtractData(Chain chain, DataSchema schema)
    {
        // Fill the repository with data
        var repository = new InMemoryDataRepository(schema.Name, schema);
        foreach (var (columnName, columnRange) in chain.Columns)
        {
            foreach (var cell in columnRange)
            {
                object? value;
                switch (cell)
                {
                    case EmptyCell:
                        value = schema.Types[columnName].GetDefaultValue();
                        break;
                    case ValueCell vcell:
                        value = vcell.Value;
                        break;
                    default:
                        continue;
                }

                repository.Add($"{columnName}_{cell.Location.ToA1()}", value);
            }
        }
        
        // Add the initialisation
        foreach (var (name, type) in schema.Types.Where(kv => kv.Key.EndsWith("-initialization")))
        {
            string colName = name.Replace("-initialization", "");

            var initData =
                chain.Initialisation.Columns.Single(l => chain.Columns[colName].Range.From.Column == l[0].Location.Column);
            
            foreach(var cell in initData)
            {
                object? value;
                switch (cell)
                {
                    case EmptyCell:
                        value = type.GetDefaultValue();
                        break;
                    case ValueCell vcell:
                        value = vcell.Value;
                        break;
                    default:
                        continue;
                }
                
                repository.Add($"{name}_{cell.Location.ToA1()}", value);
            }
        }

        return repository;
    }

    private InMemoryDataRepository ExtractData(Table table, DataSchema schema)
    {
        // Fill the repository with data
        var repository = new InMemoryDataRepository(schema.Name, schema);
        foreach (var (columnName, columnRange) in table.Columns)
        {
            foreach (var cell in columnRange)
            {
                object? value;
                switch (cell)
                {
                    case EmptyCell:
                        value = schema.Types[columnName].GetDefaultValue();
                        break;
                    case ValueCell vcell:
                        value = vcell.Value;
                        break;
                    default:
                        continue;
                }

                repository.Add($"{columnName}_{cell.Location.ToA1()}", value);
            }
        }
        
        return repository;
    }

    private DataSchema GetSchema(Chain chain)
    {
        // Look at the types of the columns
        Dictionary<string, Type> columnTypes = [];
        foreach (var (columnName, columnRange) in chain.Columns)
        {
            bool isComputed = columnRange.All(cell => cell is FormulaCell);
            if (isComputed) continue;
            
            // Get the type of the column
            var types = columnRange.Select(cell => cell.Type).ToList();
            var type = types.Distinct().Single();
            columnTypes[columnName] = type;
        }
        
        // Get the type of the initialisation
        foreach (var initialisationColumn in chain.Initialisation.Columns)
        {
            bool isEmpty = initialisationColumn.All(cell => cell is EmptyCell);
            if (isEmpty) continue;
            
            // Get the type of the column
            var types = initialisationColumn.Select(cell => cell.Type).ToList();
            var type = types.Distinct().Single(t => t is not null);

            var name = chain.Columns
                .Single(c => c.Value.Range.From.Column == initialisationColumn[0].Location.Column)
                .Key;
            
            columnTypes[$"{name}-initialization"] = type;
        }

        return new DataSchema()
        {
            Name = chain.Name,
            Types = columnTypes,
        };
    }

    private DataSchema GetSchema(Table table)
    {
        // Look at the types of the columns
        Dictionary<string, Type> columnTypes = [];
        foreach (var (columnName, columnRange) in table.Columns)
        {
            bool isComputed = columnRange.All(cell => cell is FormulaCell);
            if (isComputed) continue;
            
            // Get the type of the column
            var types = columnRange.Select(cell => cell.Type).ToList();
            var type = types.Distinct().Single();
            columnTypes[columnName] = type;
        }

        return new DataSchema()
        {
            Name = table.Name,
            Types = columnTypes,
        };
    }
}