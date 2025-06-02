using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ExcelCompiler.Generators;

public class RoslynGenerator
{
    private readonly FileWriter _writer;

    public RoslynGenerator(FileWriter writer)
    {
        _writer = writer;
    }

    public Task Generate(Project project, string outputDirectory = "output")
    {
        int count = project.Classes.Count;
        
        List<CompilationUnitSyntax> units = project.Classes.Select(Generate).ToList();
        
        return _writer.Write(outputDirectory, units);
    }

    public CompilationUnitSyntax Generate(Class @class)
    {
        var compilationUnit = CompilationUnit()
            .AddUsings(UsingDirective(IdentifierName("System")))
            .AddUsings(UsingDirective(IdentifierName("System.Collections.Generic")))
            .AddUsings(UsingDirective(IdentifierName("System.Linq")))
            .AddUsings(UsingDirective(IdentifierName("System.Text")))
            .AddUsings(UsingDirective(IdentifierName("System.Threading.Tasks")))
            .AddMembers(FileScopedNamespaceDeclaration(IdentifierName("ExcelCompiler.Generated")))
            .AddMembers(GenerateClass(@class));
        
        return compilationUnit;
    }

    private MemberDeclarationSyntax[] GenerateClass(Class @class)
    {
        IEnumerable<MemberDeclarationSyntax> properties = @class.Members.Select(Generate);
        IEnumerable<MemberDeclarationSyntax> methods = @class.Methods.Select(m => Generate(m));
        
        var classDeclaration = ClassDeclaration(@class.Name)
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddMembers(properties.ToArray())
            .AddMembers(methods.ToArray());
        
        Method constructor = @class.GenerateConstructor();
        if (constructor.Body.Length > 0)
        {
            classDeclaration = classDeclaration.AddMembers(Generate(constructor, isConstructor: true));
        }
        
        return [classDeclaration];
    }
    
    private MemberDeclarationSyntax Generate(Property property)
    {
        var propertyDeclaration = PropertyDeclaration(IdentifierName(property.Type.Name), property.Name)
            .AddModifiers(Token(SyntaxKind.PublicKeyword));

        if (property is { Getter: not null, Setter: null, Initializer: null })
        {
            return propertyDeclaration.WithExpressionBody(ArrowExpressionClause(Generate(property.Getter))).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        AccessorDeclarationSyntax getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
        AccessorDeclarationSyntax setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);

        if (property.Getter is not null)
        {
            getter = getter.WithExpressionBody(ArrowExpressionClause(Generate(property.Getter))).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            // Otherwise, close the getter with a semicolon
            getter = getter.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        if (property.Setter is not null)
        {
            setter = setter.WithExpressionBody(ArrowExpressionClause(Generate(property.Setter))).WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }
        else
        {
            // Otherwise, close the setter with a semicolon
            setter = setter.WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }


        SyntaxList<AccessorDeclarationSyntax> accessors = [getter, setter];
        propertyDeclaration = propertyDeclaration.WithAccessorList(AccessorList(accessors));
        
        // Add the initializer
        if (property.Initializer is not null)
        {
            propertyDeclaration =
                propertyDeclaration.WithInitializer(EqualsValueClause(Generate(property.Initializer)));
        }

        return propertyDeclaration;
    }

    private MemberDeclarationSyntax Generate(Method method, bool isConstructor = false)
    {
        BaseMethodDeclarationSyntax methodDeclaration = isConstructor 
            ? ConstructorDeclaration(Identifier(method.Name)) 
            : MethodDeclaration(IdentifierName(method.Type.Name), method.Name);
            
        methodDeclaration = methodDeclaration.AddModifiers(Token(SyntaxKind.PublicKeyword));
        
        // Add Parameters
        // TODO: Support Array types
        methodDeclaration = methodDeclaration
            .AddParameterListParameters(
            method.Parameters.Select(param => Parameter(Identifier(param.Name))
                .WithType(IdentifierName(param.Type.Name))).ToArray()
            );
        
        // Add the body
        List<StatementSyntax> body = method.Body.Select(Generate).ToList();

        if (body is [ReturnStatementSyntax {Expression: { } expression}])
        {
            return methodDeclaration.WithExpressionBody(ArrowExpressionClause(expression));
        }

        return methodDeclaration.WithBody(Block(body));
    }

    private StatementSyntax Generate(Statement statement)
    {
        return statement switch
        {
            Return @return => ReturnStatement(Generate(@return.ReturnExpr)),
            Assignment assignment => ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName(assignment.Variable.Name), Generate(assignment.Expression))),
            Declaration declaration => LocalDeclarationStatement(
                VariableDeclaration(IdentifierName(declaration.Variable.Type.Name)).AddVariables(
                    VariableDeclarator(declaration.Variable.Name)
                        .WithInitializer(EqualsValueClause(Generate(declaration.Expression))))
            ),
            ExpressionStatement expression => ExpressionStatement(Generate(expression.Expression)),
            _ => throw new InvalidOperationException("Something went horribly wrong."),
        };
    }
    
    private ExpressionSyntax Generate(Expression expression)
    {
        return expression switch
        {
            Constant { Type.Name: "Double" or "double" } constant => LiteralExpression(SyntaxKind.NumericLiteralExpression,
                Literal((double)constant.Value)),
            Constant { Type.Name: "String" or "string" } constant => LiteralExpression(SyntaxKind.StringLiteralExpression,
                Literal((string)constant.Value)),
            Variable variable => IdentifierName(variable.Name),
            ListExpression list when list.Members.Count != 0 => ObjectCreationExpression(
                    GenericName(Identifier("List"))
                    .WithTypeArgumentList(
                        TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(list.Type.Name)))))
                .WithInitializer(InitializerExpression(SyntaxKind.CollectionInitializerExpression)
                .AddExpressions(list.Members.Select(Generate).ToArray())),
            PropertyAccess accessor => MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                Generate(accessor.Self), IdentifierName(accessor.Name)),
            FunctionCall functionCall => GenerateFunction(functionCall),
            ObjectCreation constructor => GenerateConstructor(constructor),
            Lambda lambda => lambda.Parameters.Count == 1 
                ? SimpleLambdaExpression(Parameter(Identifier(lambda.Parameters[0].Name)), Generate(lambda.Body))
                : ParenthesizedLambdaExpression(
                    ParameterList(SeparatedList(lambda.Parameters.Select(p => 
                        Parameter(Identifier(p.Name))))), 
                    Generate(lambda.Body)),
        _ => throw new InvalidOperationException($"Expression {expression.GetType()} is not supported at the time")
        };
    }

    private ExpressionSyntax GenerateConstructor(ObjectCreation objectCreation)
    {
        // Generate the constructor as
        // new Type(Arg1, Arg2, Arg3, ..., ArgN)

        ArgumentSyntax[] arguments = objectCreation.Arguments
            .Select(Generate)
            .Select(Argument)
            .ToArray();
        
        ArgumentListSyntax argumentListSyntax = ArgumentList()
            .AddArguments(arguments);

        var invocation = ObjectCreationExpression(
            IdentifierName(objectCreation.Type.Name)
        ).WithArgumentList(argumentListSyntax);
        
        return invocation;
    }

    private ExpressionSyntax GenerateFunction(FunctionCall functionCall)
    {
        if (functionCall.Name.Length == 1 && functionCall.Arguments.Count == 2)
        {
            SyntaxKind kind = functionCall.Name switch
            {
                "+" => SyntaxKind.AddExpression,
                "-" => SyntaxKind.SubtractExpression,
                "/" => SyntaxKind.DivideExpression,
                "*" => SyntaxKind.MultiplyExpression,
                _ => throw new InvalidOperationException($"Binary operation of kind {functionCall.Name} is not supported"),
            };

            ExpressionSyntax left = Generate(functionCall.Arguments[0]);
            ExpressionSyntax right = Generate(functionCall.Arguments[1]);

            return BinaryExpression(kind, left, right);
        }
        
        // Generate the normal function call
        ExpressionSyntax invocation = functionCall.Object is null ? IdentifierName(functionCall.Name) : MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,Generate(functionCall.Object),IdentifierName(functionCall.Name));
        var invocationExpression = InvocationExpression(invocation);

        if (functionCall.Arguments.Count == 0) return invocationExpression;
        
        ArgumentSyntax[] arguments = functionCall.Arguments.Select(a => Argument(Generate(a))).ToArray();
        invocationExpression = invocationExpression.AddArgumentListArguments(arguments);

        return invocationExpression;
    }
}