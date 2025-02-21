using System.Text.Json;

namespace ExcelCompiler.Domain.Compute;

public class Function
{
    public string Raw { get; internal set; }
    
    public string Name { get; internal set; }
    
    public List<Function> Arguments { get; internal set; }

    public Function(string name, List<Function> arguments)
    {
        Name = name;
        Arguments = arguments;
    }
    
    public override string ToString() => JsonSerializer.Serialize(this);
}