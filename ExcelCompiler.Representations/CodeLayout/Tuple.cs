namespace ExcelCompiler.Representations.CodeLayout;

public record Tuple : Type
{
    public Tuple(List<Type> members) : base($"Tuple<{string.Join(",", members.Select(m => m.Name))}>")
    {
        Members = members;
    }

    private List<Type> Members { get; init; }    
}