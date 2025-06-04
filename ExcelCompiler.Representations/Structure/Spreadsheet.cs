using ExcelCompiler.Representations.Helpers;

namespace ExcelCompiler.Representations.Structure;

public class Spreadsheet
{
    public string Name { get; set; }

    private Dictionary<(int Row, int Column), Cell> _cells { get; } = [];
    
    public List<Table> Tables { get; } = [];
    
    public Cell this[int row, int column]
    {
        get => _cells[(row, column)];
        set => _cells[(row, column)] = value;
    }

    public Cell this[References.Location location]
    {
        get
        {
            if(location.Spreadsheet is not null && location.Spreadsheet != this.Name)
            {
                throw new IndexOutOfRangeException("Location is not on this spreadsheet.");
            }
            
            return !_cells.TryGetValue((location.Row, location.Column), out var cell) ? new EmptyCell(location) : cell;
        }
        set
        {
            if (location.Spreadsheet is not null && location.Spreadsheet != this.Name)
            {
                throw new IndexOutOfRangeException("Location is not on this spreadsheet.");
            }
            
            _cells[(location.Row, location.Column)] = value;
        }
    }

    public Cell[,] this[References.Range range] => range.ToArray().Map(i => this[i]);

    public Spreadsheet(string name)
    {
        Name = name;
    }
    
    public IEnumerable<Cell> Cells => _cells.Values;
}