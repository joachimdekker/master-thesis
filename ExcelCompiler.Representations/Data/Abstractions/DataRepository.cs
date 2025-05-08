namespace ExcelCompiler.Representations.Data;

public interface IDataRepository
{
    public Guid Id { get; }
    public string Name { get; init; }
    public DataSchema Schema { get; init; }
    public ProviderType Provider { get; init; }
}