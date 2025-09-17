using ExcelCompiler.Representations.Helpers;

namespace ExcelCompiler.Representations.Compute;

public record Type
{
    public string Name { get; init; }

    public object DefaultValue { get; init; } = null!;

    public Type(string name)
    {
        Name = name;
    }

    public Type(System.Type type) : this(type.Name.ToLower())
    {
        DefaultValue = type.GetDefaultValue();
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}