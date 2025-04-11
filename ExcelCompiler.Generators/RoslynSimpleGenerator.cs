using ExcelCompiler.Domain.Compute;
using Microsoft.Extensions.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Range = ExcelCompiler.Domain.Compute.Range;

namespace ExcelCompiler.Generators;

public class RoslynSimpleGenerator : IFileGenerator
{
    private readonly ILogger<RoslynSimpleGenerator> _logger;
    
    public RoslynSimpleGenerator(ILogger<RoslynSimpleGenerator> logger)
    {
        _logger = logger;
    }
    
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
        foreach (var cell in graph.TopologicalSortedByCell().Reverse())
        {
            yield return LocalDeclarationStatement(
                VariableDeclaration(PredefinedType(Token(SyntaxKind.IntKeyword)))
                .AddVariables(VariableDeclarator(cell.Location.ToA1()).WithInitializer(EqualsValueClause(GenerateCode(cell))))
            );
        }
        
        // Console.Log the roots
        foreach (var root in graph.Roots)
        {
            yield return ConsoleLog(IdentifierName(root.Location.ToA1()));
        }
    }

    private ExpressionSyntax GenerateCode(ComputeUnit unit) => unit switch
    {
        ConstantValue<string> constant => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(constant.Value)),
        ConstantValue<decimal> constant => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(constant.Value)),
        ConstantValue<int> constant => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(constant.Value)),
        ConstantValue<double> constant => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(constant.Value)),
        Reference reference => IdentifierName(reference.CellReference.ToA1()),
        Function { Name: "+" } function =>
            BinaryExpression(SyntaxKind.AddExpression, GenerateCode(function.Dependencies[0]), GenerateCode(function.Dependencies[1])),
        Function { Name: "-" } function =>
            BinaryExpression(SyntaxKind.SubtractExpression, GenerateCode(function.Dependencies[0]), GenerateCode(function.Dependencies[1])),
        Function { Name: "SUM", Dependencies: [Range range]} => InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, 
            ArrayCreationExpression(
                ArrayType(PredefinedType(Token(SyntaxKind.IntKeyword)))
                    .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))
                ).WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression).AddExpressions(range.GetLocations().Select(l => IdentifierName(l.ToA1())).ToArray<ExpressionSyntax>())),
            IdentifierName("Sum"))),
        _ => throw new NotSupportedException("Unknown type " + unit.GetType())
    };
}