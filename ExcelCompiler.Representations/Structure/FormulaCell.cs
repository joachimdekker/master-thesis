using ExcelCompiler.Representations.Structure.Formulas;

namespace ExcelCompiler.Representations.Structure;

public record FormulaCell : Cell
{
    public FormulaCell(Location Location, Type Type, string Raw) : base(Location, Type)
    {
        this.Raw = Raw;
        Formula = FormulaExpression.Parse(Raw, new FormulaContext()
        {
            Spreadsheet = Location.Spreadsheet!,
        });
        
    }

    public FormulaExpression Formula { get; init; }
    public string Raw { get; init; }
}