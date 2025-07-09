namespace ExcelCompiler.Representations.Compute;

public record ListOf : Type
{
    public ListOf(Type memberType) : base($"List<{memberType.Name}>")
    {
        MemberType = memberType;
    }

    public Type MemberType { get; set; }
}