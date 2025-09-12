using System.Collections;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
        int endOffset = range.End.IsFromEnd ? RowCount - (range.End.Value + 1) : range.End.Value - 1;

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

public class LineSelection : IList<Cell>
{
    private IList<Cell> _line;

    public LineSelection(IList<Cell> line)
    {
        _line = line;
    }
    
    public static implicit operator LineSelection(List<Cell> cells)
    {
        // Get the range
        return new LineSelection(cells);
    }

    public Range Range => new(this[0].Location, this[^1].Location);

    public bool TryGetFirstNonEmptyCell([NotNullWhen(true)] out Cell? cell)
    {
        cell = _line.FirstOrDefault(c => c is not EmptyCell);
        return cell is not null;
    }
    
    public IEnumerator<Cell> GetEnumerator() => _line.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_line).GetEnumerator();

    public void Add(Cell item) => _line.Add(item);

    public void Clear() => _line.Clear();

    public bool Contains(Cell item) => _line.Contains(item);

    public void CopyTo(Cell[] array, int arrayIndex) => _line.CopyTo(array, arrayIndex);

    public bool Remove(Cell item) => _line.Remove(item);

    public int Count => _line.Count;

    public bool IsReadOnly => _line.IsReadOnly;

    public int IndexOf(Cell item) => _line.IndexOf(item);

    public void Insert(int index, Cell item) => _line.Insert(index, item);

    public void RemoveAt(int index) => _line.RemoveAt(index);

    public Cell this[int index]
    {
        get => _line[index];
        set => _line[index] = value;
    }
}