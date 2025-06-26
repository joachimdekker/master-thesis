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

    private IDataRepository ExtractData(Construct construct, DataSchema schema) => construct switch
    {
        Table t => ExtractData(t, (schema as TableDataSchema)!),
        Chain c => ExtractData(c, (schema as ChainDataSchema)!),
        _ => throw new InvalidOperationException("Unsupported construct type.")
    };

    private DataSchema GetSchema(Construct construct) => construct switch
    {
        Table t => GetSchema(t),
        Chain c => GetSchema(c),
        _ => throw new InvalidOperationException("Unsupported construct type.")
    };

    private ChainRepository ExtractData(Chain chain, ChainDataSchema schema)
    {
        // Fill the repository with data
        var dataStorage = schema.Data.Select((name, type, _) =>
        {
            var columnRange = chain.Columns[name];
            var columnData = columnRange.Select(cell => cell switch
            {
                EmptyCell => type.GetDefaultValue(),
                ValueCell vcell => vcell.Value,
                _ => throw new InvalidOperationException("Unsupported cell type.")
            }).ToList();
            return (name, columnData);
        }).ToDictionary();

        // Add the initialisation
        var initialization = schema.Initialization.Select((name, type, _) =>
        {
            var initData = chain.Initialisation.Columns
                .Single(l => chain.Columns[name].Range.From.Column == l[0].Location.Column)
                .Select(l =>
                {
                    return l switch
                    {
                        EmptyCell => type.GetDefaultValue(),
                        ValueCell vcell => vcell.Value,
                        _ => throw new InvalidOperationException("Unsupported cell type.")
                    };
                })
                .ToList();

            return (name, initData);
        }).ToDictionary();

        return new ChainRepository(
            name: schema.Name,
            schema: schema,
            data: dataStorage,
            initialization: initialization
        );
    }

    private TableRepository ExtractData(Table table, TableDataSchema schema)
    {
        // Fill the repository with data
        // Get the data from every column and fill the repository

        var columns = schema.Columns.Select((name, type, _) =>
        {
            var columnRange = table.Columns[name];
            var columnData = columnRange.OfType<ValueCell>().Select(cell => cell.Value).ToList();
            return (name, columnData);
        }).ToDictionary();

        return new TableRepository()
        {
            Name = table.Name,
            Schema = schema,
            Columns = columns,
        };
    }

    private DataSchema GetSchema(Chain chain)
    {
        // Look at the types of the columns
        Dictionary<string, Type> dataTypes = [];
        foreach (var (columnName, columnRange) in chain.Columns)
        {
            bool isComputed = columnRange.All(cell => cell is FormulaCell);
            if (isComputed) continue;
            
            // Get the type of the column
            var types = columnRange.Select(cell => cell.Type).ToList();
            var type = types.Distinct().Single();
            dataTypes[columnName] = type;
        }
        
        // Get the type of the initialisation
        Dictionary<string, Type> initializationTypes = [];
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
            
            initializationTypes[name] = type;
        }

        return new ChainDataSchema()
        {
            Name = chain.Name,
            Data = dataTypes,
            Initialization = initializationTypes,
        };
    }

    private TableDataSchema GetSchema(Table table)
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

        return new TableDataSchema()
        {
            Name = table.Name,
            Columns = columnTypes,
        };
    }
}