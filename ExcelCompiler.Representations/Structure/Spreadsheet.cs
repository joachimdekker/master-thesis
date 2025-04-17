using ExcelCompiler.Domain.Compute;

namespace ExcelCompiler.Domain.Structure;

public class Spreadsheet
{
    public string Name { get; set; }

    private Dictionary<(int Row, int Column), Cell> _cells { get; } = [];
    
    public Cell this[int row, int column]
    {
        get => _cells[(row, column)];
        set => _cells[(row, column)] = value;
    }

    public Cell this[Location location]
    {
        get
        {
            if(location.Spreadsheet is not null && location.Spreadsheet != this)
            {
                throw new IndexOutOfRangeException("Location is not on this spreadsheet.");
            }
            
            return _cells[(location.Row, location.Column)];
        }
        set
        {
            if (location.Spreadsheet is not null && location.Spreadsheet != this)
            {
                throw new IndexOutOfRangeException("Location is not on this spreadsheet.");
            }
            
            _cells[(location.Row, location.Column)] = value;
        }
    }

    public Spreadsheet(string name)
    {
        Name = name;
    }
    
    public IEnumerable<Cell> Cells => _cells.Values;
}