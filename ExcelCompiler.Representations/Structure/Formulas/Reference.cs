using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure.Formulas;

public record Reference : FormulaExpression
{
    public static Reference Parse(References.Reference reference)
    {
        return reference switch
        {
            References.Location locationRef => new CellReference(locationRef),
            Range range => new RangeReference(range),
            References.TableReference tableRef => new TableReference(tableRef),
            _ => throw new InvalidOperationException($"Reference type {reference.GetType()} is not supported."),
        };
    }
}

public record RangeReference(Range Reference) : Reference;

public record CellReference(References.Location Reference) : Reference;

public record TableReference(References.TableReference Reference) : Reference;