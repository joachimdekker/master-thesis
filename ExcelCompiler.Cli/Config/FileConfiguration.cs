namespace ExcelCompiler.Cli.Config;

public class FileConfiguration
{
    public required string Location { get; set; }
    public required string Type { get; set; }
    public required List<string> Inputs { get; set; }
    public required List<string> Outputs { get; set; }
}