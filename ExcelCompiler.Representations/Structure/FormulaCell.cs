using ExcelCompiler.Representations.Structure.Formulas;

namespace ExcelCompiler.Representations.Structure;

public record FormulaCell : Cell
{
    public FormulaCell(References.Location location, Type type, string raw) : base(location, type)
    {
        Raw = raw;
        Formula = FormulaExpression.Parse(raw, new FormulaContext()
        {
            Spreadsheet = location.Spreadsheet!,
        });
    }

    public FormulaExpression Formula { get; init; }
    public string Raw { get; init; }
}