using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;

namespace ExcelCompiler.Passes.Preview.Compute;

[CompilerPass]
public class ExtractStructureData
{
    public ComputeGrid Transform(ComputeGrid grid)
    {
        // For every construct in the grid, extract the structure data
        List<Construct> constructs = grid.Structures.Select(s => Transform(grid, s)).ToList();
        
        return grid with
        {
            Structures = constructs
        };
    }

    public Construct Transform(ComputeGrid grid, Construct construct)
    {
        switch (construct)
        {
            case Table table:
                TableStructureData tableData = ExtractTableStructureData(grid, table);
                return table with
                {
                    StructureData = tableData,
                };
            case Chain chain:
                ChainStructureData chainData = ExtractChainStructureData(grid, chain);
                return chain with
                {
                    StructureData = chainData,
                };
            default:
                throw new InvalidOperationException();
        }
    }

    private ChainStructureData ExtractChainStructureData(ComputeGrid grid, Chain chain)
    {
        // Get the data columns, then get the initializers
        IEnumerable<ColumnData> columns =
            from column in chain.Columns.OfType<DataChainColumn>()
            let data = column.Location.Select(l => grid[l]).ToList()
            select new ColumnData()
            {
                ColumnId = column.Name,
                Type = column.Type,
                Data = data,
            };

        IEnumerable<ColumnData> initializers =
            from column in chain.Columns.OfType<RecursiveChainColumn>()
            let initializer = column.Location.Where((_, i) => i < column.NoBaseCases).Select(l => grid[l]).ToList()
            select new ColumnData()
            {
                ColumnId = column.Name,
                Type = column.Type,
                Data = initializer,
            };
        
        return new ChainStructureData()
        {
            StructureId = chain.Name,
            Columns = columns.ToList(),
            Initialisations = initializers.ToList(),
        };
    }

    private TableStructureData ExtractTableStructureData(ComputeGrid grid, Table table)
    {
        IEnumerable<ColumnData> columns =
            from column in table.Columns 
            where column.ColumnType is not TableColumn.TableColumnType.Computed 
            let data = column.Location.Select(l => grid[l]).ToList() 
            select new ColumnData()
            {
                ColumnId = column.Name, 
                Type = column.Type, 
                Data = data,
            };
        
        return new TableStructureData()
        {
            StructureId = table.Name,
            Columns = columns.ToList(),
        };
    }
}