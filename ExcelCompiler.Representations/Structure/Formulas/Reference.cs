namespace ExcelCompiler.Representations.Structure.Formulas;

public record Reference : FormulaExpression
{
    public static Reference Parse(Domain.Structure.Reference reference)
    {
        return reference switch
        {
            Location locationRef => new CellReference(locationRef),
            Range range => new RangeReference(range),
            Structure.TableReference tableRef => new TableReference(tableRef),
            _ => throw new InvalidOperationException($"Reference type {reference.GetType()} is not supported."),
        };
    }
}

public record RangeReference(Range Reference) : Reference;

public record CellReference(Location Reference) : Reference;

public record TableReference(Representations.Structure.TableReference Reference) : Reference;