using ExcelCompiler.Passes.Helpers;

namespace ExcelCompiler.Representations.CodeLayout.TopLevel;

public record Type
{
    private static readonly List<string> BuiltInTypes = ["var", "void", "int", "string", "bool", "double"];
    public Type(string Name)
    {
        Name = BuiltInTypes.Contains(Name.ToLower()) 
            ? Name.ToLower() 
            : Name.ToPascalCase();

        this.Name = Name;
    }

    public static readonly Type Void = new("void");

    public static readonly Type Derived = new("var");

    public Type(System.Type type) : this(type.Name)
    {
    }

    public string Name { get; init; }
}
