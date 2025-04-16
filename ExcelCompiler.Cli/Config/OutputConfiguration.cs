namespace ExcelCompiler.Cli.Config;

public record OutputConfiguration
{
    public string Location { get; init; } = string.Empty;
    public string Language { get; init; } = "cs";
}