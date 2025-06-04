using System.Diagnostics.CodeAnalysis;

namespace ExcelCompiler.Representations.References;

public record TableReference : Reference
{
    public TableReference() { }

    [SetsRequiredMembers]
    public TableReference(string name, string column)
    {
        TableName = name;
        ColumnNames = [column];
    }

    [SetsRequiredMembers]
    public TableReference(string name, params List<string> columns)
    {
        TableName = name;
        ColumnNames = columns;
    }

    public required string TableName { get; init; }
    
    public required List<string> ColumnNames { get; init; }
}