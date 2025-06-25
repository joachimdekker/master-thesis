using ExcelCompiler.Representations.CodeLayout.TopLevel;

namespace ExcelCompiler.Representations.CodeLayout;

public record Project
{
    public string Name { get; init; }

    public List<Class> Input { get; init; } = new();

    public List<Class> Types { get; init; } = new();

    public List<Class> Classes { get; init; } = new();
}
