using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;

namespace ExcelCompiler.Representations.Structure;

public class Spreadsheet
{
    public string Name { get; set; }

    private Dictionary<(int Row, int Column), Cell> _cells { get; } = [];
    
    public List<ExcelTable> Tables { get; } = [];
    
    public Cell this[int row, int column]
    {
        get => _cells[(row, column)];
        set => _cells[(row, column)] = value;
    }

    public Cell this[Location location]
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

    public Selection this[References.Range range]
    {
        get
        {
            List<List<Cell>> cells = range.ToList().Select(ls => ls.Select(l => this[l]).ToList()).ToList();

            return new Selection(cells, range);
        }
    }

    public Spreadsheet(string name)
    {
        Name = name;
    }
    
    public IEnumerable<Cell> Cells => _cells.Values;
}