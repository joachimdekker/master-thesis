namespace ExcelCompiler.Domain.Compute;

public class Reference : Function
{
    public int Row { get; }
    
    public int Column { get; }
    
    public Reference(int row, int column) : base("R" + row + "C" + column, [])
    {
        Row = row;
        Column = column;
    }

    public Reference(string raw) : base(raw, new List<Function>())
    {
    }
}