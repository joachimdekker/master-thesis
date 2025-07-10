using ExcelCompiler.Passes.Helpers;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using Assignment = ExcelCompiler.Representations.CodeLayout.Expressions.Assignment;

namespace ExcelCompiler.Representations.CodeLayout.TopLevel;

public record Class : Type
{
    public Class(string Name, List<Property> Members, List<Method> Methods) : base(Name)
    {
        this.Members = Members;
        this.Methods = Methods;
    }
    
    public List<Property> Members { get; init; }
    public List<Method> Methods { get; init; }

    public Method GenerateConstructor()
    {
        // Get all members that can be set
        var settableMembers = Members.Where(x => x is not { Getter: not null, Setter: null } && x.Initializer is null).ToArray();
        
        // Generate arguments
        var arguments = settableMembers.Select(x => new Variable(x.Name.ToCamelCase(), x.Type)).ToArray();
        
        // Generate body
        var body = settableMembers.Select(x =>  new ExpressionStatement(new Assignment(new Variable(x.Name.ToPascalCase(), x.Type),
            new Variable(x.Name.ToCamelCase(), x.Type))));

        // Generate constructor
        return new Method(Name, arguments, body.ToArray<Statement>());
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}