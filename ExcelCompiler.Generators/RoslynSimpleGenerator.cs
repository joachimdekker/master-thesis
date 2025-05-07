using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Structure;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Location = ExcelCompiler.Representations.Structure.Location;
using Range = ExcelCompiler.Representations.Structure.Range;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;

namespace ExcelCompiler.Generators;

public class RoslynSimpleGenerator : IFileGenerator
{
    private readonly ILogger<RoslynSimpleGenerator> _logger;
    
    public RoslynSimpleGenerator(ILogger<RoslynSimpleGenerator> logger)
    {
        _logger = logger;
    }
    
    private string LocationToVarName(Location loc) => loc.Spreadsheet!.Replace(" ", "") + loc.ToA1();
    
    public async Task Generate(SupportGraph graph, Stream outputStream, CancellationToken cancellationToken = default)
    {
        // Create Main method
        IEnumerable<StatementSyntax> body = Generate(graph);
        var mainMethod = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "Main")
            .AddModifiers(Token(SyntaxKind.StaticKeyword))
            .AddParameterListParameters(
                Parameter(Identifier("args"))
                    .WithType(ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                    .WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))))
            )
            .WithBody(Block(body));

        // Create Program class
        var programClass = ClassDeclaration("Program")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(mainMethod);

        // Create Compilation Unit
        var compilationUnit = CompilationUnit()
            .AddUsings(UsingDirective(IdentifierName("System")))
            .AddMembers(programClass)
            .NormalizeWhitespace();

        // Output the final code as string
        var code = compilationUnit.ToFullString();
        _logger.LogDebug("Generated code:\n\n{Code}", code);
        
        // Write the code to the file
        await using StreamWriter writer = new StreamWriter(outputStream);
        await writer.WriteAsync(code);
        
        _logger.LogInformation("Finished writing to file");
    }

    private StatementSyntax ConsoleLog(ExpressionSyntax expression)
    {
        return ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Console"),
                    IdentifierName("WriteLine")
                )
            ).WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(expression))))
        );
    }

    private IEnumerable<StatementSyntax> Generate(SupportGraph graph)
    {
        // Create a variable per cell and start at the roots
        foreach (var cell in graph.EntryPointsOfCells().Reverse())
        {
            yield return LocalDeclarationStatement(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                .AddVariables(VariableDeclarator(LocationToVarName(cell.Location)).WithInitializer(EqualsValueClause(GenerateCode(cell, graph))))
            );
        }
        
        // Console.Log the roots
        foreach (var root in graph.Roots)
        {
            yield return ConsoleLog(IdentifierName(LocationToVarName(root.Location)));
        }
    }

    private ExpressionSyntax GenerateCode(ComputeUnit unit, SupportGraph graph) => unit switch
    {
        ConstantValue<string> constant => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(constant.Value)),
        ConstantValue<decimal> constant => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(constant.Value)),
        ConstantValue<int> constant => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(constant.Value)),
        ConstantValue<double> constant => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(constant.Value)),
        CellReference reference => IdentifierName(LocationToVarName(reference.Reference)),
        Function { Name: "+" } function =>
            BinaryExpression(SyntaxKind.AddExpression, GenerateCode(function.Dependencies[0], graph), GenerateCode(function.Dependencies[1], graph)),
        Function { Name: "-" } function =>
            BinaryExpression(SyntaxKind.SubtractExpression, GenerateCode(function.Dependencies[0], graph), GenerateCode(function.Dependencies[1], graph)),
        Function { Name: "SUM", Dependencies: [RangeReference range]} => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
            ArrayCreationExpression(
                ArrayType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))
                ).WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression).AddExpressions(range.Dependencies.Select(l => IdentifierName(LocationToVarName(l.Location))).ToArray<ExpressionSyntax>())),
            IdentifierName("Sum"))),
        Function { Name: "SUM", Dependencies: [TableReference tableRef]} => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
            ArrayCreationExpression(
                ArrayType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))
            ).WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression).AddExpressions(GetTableDeps(tableRef.Reference, graph).Select(l => IdentifierName(LocationToVarName(l))).ToArray<ExpressionSyntax>())),
            IdentifierName("Sum"))),
        _ => throw new NotSupportedException("Unknown type " + unit.GetType())
    };

    private IEnumerable<Location> GetTableDeps(Representations.Structure.TableReference tableRefReference, SupportGraph graph)
    {
        // Get the column in the graph
        var table = graph.Tables.Single(t => t.Name == tableRefReference.TableName);
        
        // Get the column
        var column = table.Columns.Single(c => c.Name == tableRefReference.ColumnName);
        
        // Get the locations
        var columnIndex = table.Columns.IndexOf(column);
        var columnPlace = table.Location.From.Column + columnIndex;
        
        // Create a new range
        Range range = new(from: new Location
            {
                Column = columnPlace,
                Row = table.Location.From.Row,
                Spreadsheet = table.Location.Spreadsheet
            },
            to: new Location
            {
                Column = columnPlace,
                Row = table.Location.To.Row,
                Spreadsheet = table.Location.Spreadsheet
            });
        
        // Get the locations
        return range.GetLocations();
    }
}