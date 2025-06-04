namespace ExcelCompiler.Representations.Structure;

public record ValueCell(References.Location Location, Type Type, object Value) : Cell(Location, Type);

public record ValueCell<T> : ValueCell
{
    public new T Value
    {
        get => (T) base.Value;
        init => base.Value = value!;
    }
    
    public ValueCell(T value, References.Location location) : base(location, typeof(T), value!)
    {
    }

    public void Deconstruct(out References.Location location, out T value)
    {
        location = Location;
        value = Value;
    }
}