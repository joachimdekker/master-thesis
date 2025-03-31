using System.Runtime.InteropServices.JavaScript;
using ExcelCompiler.Domain.Spreadsheet;

namespace ExcelCompiler.Domain.Compute;

public class Reference : ComputeUnit
{
    public Location CellReference { get; internal set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Reference"/> class.
    /// </summary>
    /// <param name="reference">The reference to another cell.</param>
    /// <param name="location">The location of the reference.</param>
    /// <example>There is a reference in cell <c>A1</c> that references to <c>B2</c>, then we use
    /// <code>
    /// var reference = new Reference(Location.FromA1("B2"), Location.FromA1("A1"));
    /// </code>
    /// </example>
    public Reference(Location reference, Location location) : base(location)
    {
        CellReference = reference;
    }
}