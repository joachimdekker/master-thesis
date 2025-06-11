namespace ExcelCompiler.Representations.References;

public record AxisPosition(int Index, bool IsAbsolute = false) : IComparable<AxisPosition>
{
    public static implicit operator int(AxisPosition position) => position.Index;

    public static AxisPosition operator +(AxisPosition position, int value) => 
        position with { Index = position.Index + value };

    public static AxisPosition operator +(int value, AxisPosition position) => 
        position with { Index = position.Index + value };
    
    public static AxisPosition operator +(AxisPosition position, AxisPosition other) => 
        position with { Index = position.Index + other.Index };

    public static AxisPosition operator -(AxisPosition position, AxisPosition other) =>
        position with { Index = position.Index - other.Index };

    public static AxisPosition operator -(AxisPosition position, int value) => 
        position with { Index = position.Index - value };

    // public static AxisPosition operator -(int value, AxisPosition position) => 
    //     position with { Index = value - position.Index };
    
    public int CompareTo(AxisPosition? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Index.CompareTo(other.Index);
    }
}