namespace ExcelCompiler.Representations.Data.Preview;

public interface IDataRepository
{
    public string Name { get; init; }
    public DataSchema Schema { get; init; }
}