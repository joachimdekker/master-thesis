using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute.Specialized;
using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Passes.Code;

[CompilerPass]
public class ExtractDataClasses
{
    private readonly ComputeExpressionConverter _computeExpressionConverter;
    
    public ExtractDataClasses(ComputeExpressionConverter computeExpressionConverter)
    {
        _computeExpressionConverter = computeExpressionConverter;
    }

    public List<Class> ExtractTypes(List<Table> tables)
    {
        // Extract the types from the columnar data types for now.
        return (
            from table in tables
            let properties = (
                from column in table.Columns
                let @type = new Type(column.Type.Name)
                select new Property(column.Name, @type)
                {
                    Getter = column.ColumnType is TableColumn.TableColumnType.Computed ? _computeExpressionConverter.Transform(column.Computation!) : null,
                }
            ).ToList()
            select new Class($"{table.Name} Item", properties, [])
        ).ToList();
    }
}