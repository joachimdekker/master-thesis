using ExcelCompiler.Representations.CodeLayout.Expressions;

namespace ExcelCompiler.Representations.CodeLayout.Statements;

public record Conditional : Statement
{
    /// <summary>
    /// Gets the cases and the accompanying block of the conditional statement.
    /// </summary>
    public virtual required List<(Expression, List<Statement>)> Cases { get; init; }

    public virtual List<Statement>? Default { get; set; }
}
