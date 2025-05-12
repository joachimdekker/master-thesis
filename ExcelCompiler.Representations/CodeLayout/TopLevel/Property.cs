using ExcelCompiler.Passes.Helpers;
using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.TopLevel;

public record Property(string Name, Type Type)
{
    public Expression? Initializer { get; set; }
    public Expression? Getter { get; set; }
    public Expression? Setter { get; set; }
    public string Name { get; init; } = Name.ToPascalCase();
}