using ExcelCompiler.Passes.Code;
using ExcelCompiler.Passes.Helpers;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using Tuple = ExcelCompiler.Representations.CodeLayout.Tuple;
using Type = ExcelCompiler.Representations.CodeLayout.Type;

namespace ExcelCompiler.Passes.Preview.Code;

[CompilerPass]
public class InjectMemoization
{
    public Project Transform(Project project)
    {
        var memoizationProjectTransformer = new MemoizationProjectTransformer(project);
        return memoizationProjectTransformer.Transform(project);
    }
}

public record MemoizationProjectTransformer(Project Transformee) : UnitProjectTransformer
{
    protected override Class Class(Class @class, List<Method> methods, List<Property> properties)
    {
        if (@class.Name == "Main") return @class;
        
        // Add memoization to the class if needed
        List<(Method m, Property? p)> memoizedMethods = methods.Select(m => Method(@class, m, m.Body.ToList())).ToList();
        
        return @class with
        {
            Methods = memoizedMethods.Select(x => x.m).ToList(),
            Members = properties.Concat(memoizedMethods.Select(x => x.p).Where(m => m is not null).Cast<Property>()).ToList(),
        };
    }

    private (Method, Property?) Method(Class @class, Method method, List<Statement> statements)
    {
        // Count how many statements call this method recursively
        var recursiveCounter = new RecursiveCounter(@class, method, Transformee);
        int recursiveCalls = recursiveCounter.Transform(method);
        
        if (recursiveCalls == 0)
            return (method, null!);
        
        var (memoizedDictionary, body) = AddMemoizationToMethod(method);

        return (method with { Body = body.ToArray() }, memoizedDictionary);
    }

    private static (Property memoizedDictionary, List<Statement> body) AddMemoizationToMethod(Method method)
    {
        // Add memoization
        // Add memoization to the method
        // The method should be like:
        // if present in Dictionary? -> return
        // else -> Compute, add to dictionary, return
        
        
        Type keyType = method.Parameters.Length == 1 ? method.Parameters[0].Type : new Tuple(method.Parameters.Select(p => p.Type).ToList());
        Map type = new Map(keyType, method.Type);
        
        Property memoizedDictionary = new Property($"_{method.Name.ToCamelCase()}Memoization", type)
        {
            Initializer = new ObjectCreation(type, [])
        };
        
        Variable dictionary = new Variable(memoizedDictionary.Name, type);
        Expression keyCreation = method.Parameters.Length == 1
            ? method.Parameters[0]
            : new ObjectCreation(keyType,
                method.Parameters.Select(p => new Variable(p.Name)).Cast<Expression>().ToList());
        Statement keyDeclaration = new Declaration(new Variable("key", keyType), keyCreation);
        Statement ifPresent =
            new If(
                new FunctionCall(dictionary, "ContainsKey",
                    [new Variable("key")]),
                [new Return(new MapAccessor(dictionary, new Variable("key")))]);
        
        List<Statement> calculation = method.Body.Where(s => s is not Representations.CodeLayout.Statements.Return).ToList();
        Expression returnExpression = (method.Body[^1] as Return)!.ReturnExpr;
        
        List<Statement> body =
        [
            keyDeclaration,
            ifPresent,
            ..calculation,
            new Declaration(new Variable("result", method.Type), returnExpression),
            new ExpressionStatement(new FunctionCall(dictionary, "Add", [new Variable("key"), new Variable("result")])),
            new Return(new Variable("result", method.Type))
        ];
        return (memoizedDictionary, body);
    }

    // private int CountRecursiveCall(Class c, Method method, Statement statement)
    // {
    //     List<Expression> expressions = statement switch
    //     {
    //         ExpressionStatement expressionStatement => [expressionStatement.Expression],
    //         Assignment assignment => [assignment.Expression],
    //         If @if => [@if.Condition],
    //         Return @return => [@return.ReturnExpr],
    //         Declaration declaration => [declaration.Expression],
    //
    //         _ => throw new InvalidOperationException()
    //     };
    //     return expressions.Sum(e => CountRecursiveCall(c, method, e));
    // }
    //
    // private int CountRecursiveCall(Class c, Method method, Expression expression)
    // {
    //     if (expression is not FunctionCall functionCall)
    //         return 0;
    //
    //     if ((functionCall.Object?.Type is null || functionCall.Object.Type == c) && functionCall.Name == method.Name) 
    //         return 1;
    //     
    //     bool seen = _seen.Add($"{functionCall.Object?.Type.Name}:{functionCall.Name}");
    //     if (seen) 
    //         return 0;
    //     
    //     // Get methods with the same name
    //     var methods = Transformee.Classes.SelectMany(c => c.Methods).Where(m => m.Name == functionCall.Name).ToList();
    //     
    //     // Count how many of them are recursive
    //     int recursiveCalls = methods.SelectMany(m => m.Body.Select(s => CountRecursiveCall(c, method, s))).Sum();
    //     
    //     return recursiveCalls;
    // }
}

file record RecursiveCounter(Class TargetClass, Method TargetMethod, Project TargetProject) : BulkTransformer<int>
{
    private readonly HashSet<string> _seen = new();
    
    protected override int Constant(Constant constant)
    {
        return 0;
    }

    protected override int Variable(Variable variable)
    {
        return 0;
    }

    protected override int Combine(IEnumerable<int> elements)
    {
        return elements.Sum();
    }

    protected override int FunctionCall(FunctionCall functionCall, List<int> arguments)
    {
        if ((functionCall.Object is null || functionCall.Object.Type == TargetClass) &&
            functionCall.Name == TargetMethod.Name)
        {
            return 1;
        }
        
        bool seen = _seen.Add($"{functionCall.Object?.Type.Name ?? TargetClass.Name}:{functionCall.Name}");
        if (seen)
        {
            return Combine(arguments);
        }
        
        // Get methods with the same name
        // Get methods with the same name
        var methods = TargetProject.Classes.SelectMany(c => c.Methods).Where(m => m.Name == functionCall.Name).ToList();
        
        // Count how many of them are recursive
        int recursiveCalls = methods.Sum(Transform);

        return recursiveCalls + Combine(arguments);
    }
}
