using System.Data;
using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;
using Range = ExcelCompiler.Representations.References.Range;

namespace ExcelCompiler.Representations.Structure;

public class Selection
{
    public static Selection Empty => new Selection(new List<List<Cell>>(), new Range(new Location(null!, 0, 0), new Location(null!, 0, 0)));
    
    private readonly List<List<Cell>> _cells;
    
    public Range Range { get; init; }
    
    public Selection(List<List<Cell>> cells, Range range)
    {
        _cells = cells;
        Range = range; 
    }

    public Cell this[int row, int col]
    {
        get => _cells[row][col];
        set => _cells[row][col] = value;
    }
    
    public int ColumnCount => _cells[0].Count;
    public int RowCount => _cells.Count;

    public List<List<Cell>> Rows => _cells;

    public List<List<Cell>> Columns => Transpose().Rows;
    
    public List<Cell> GetRow(int i) => _cells[i];

    public Selection GetRows(System.Range range)
    {
        List<List<Cell>> selection = _cells[range];
        
        int startOffset = range.Start.IsFromEnd ? RowCount - range.Start.Value : range.Start.Value;
        int endOffset = range.End.IsFromEnd ? RowCount - range.End.Value : range.End.Value;

        Location start = Range.From with
        {
            Row = Range.From.Row + startOffset,
        };
        Location end = Range.To with
        {
            Row = Range.From.Row + endOffset,
        };
        
        // Get a change in range
        return new Selection(selection, new Range(start, end));
    }

    public Selection Transpose()
    {
        return new Selection(
            _cells.Transpose(),
            new Range(Range.From, Range.To)
        );
    }

    public List<Cell> GetColumn(int i) => _cells.Select(row => row[i]).ToList();

    public static implicit operator Selection(List<Cell> cells)
    {
        // Get the location
        Location start = cells[0].Location;
        Location end = cells[^1].Location;
        
        // Check if the selection is vertical or horizontal
        if (start.Column == end.Column)
        {
            // Vertical
            return new Selection(cells.Select<Cell,List<Cell>>(c => [c]).ToList(), new Range(start, end));
        }
        
        // Get the range
        return new Selection([cells], new Range(start, end));
    }
}