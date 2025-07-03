using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;

namespace ExcelCompiler.Passes.Code;

public abstract record ProjectTransformer<TRes, TClass, TMethod, TProperty, TStat, TExpr>
{
    protected abstract TRes Project(Project project, List<TClass> classes);

    protected abstract TClass Class(Class @class, List<TMethod> methods, List<TProperty> properties);
    
    protected abstract TMethod Method(Method method, List<TStat> statements);
    
    protected abstract TProperty Property(Property property, TExpr? getter, TExpr? setter, TExpr? initializer);
    
    protected abstract TStat Declaration(Declaration declaration, TExpr expression);
    
    protected abstract TStat Assignment(Assignment assignment, TExpr expression);
    
    protected abstract TStat Return(Return returnStatement, TExpr expression);
    
    protected abstract TStat ExpressionStatement(ExpressionStatement expressionStatement, TExpr expression);

    protected abstract TStat If(If ifStatement, TExpr condition, IEnumerable<TStat> body, IEnumerable<TStat>? @else);
    
    protected abstract TExpr Constant(Constant constant);
    
    protected abstract TExpr Variable(Variable variable);
    
    protected abstract TExpr FunctionCall(FunctionCall functionCall, List<TExpr> arguments);
    
    protected abstract TExpr ListExpression(ListExpression listExpression, List<TExpr> members);
    
    protected abstract TExpr Lambda(Lambda lambda, TExpr body);
    
    protected abstract TExpr PropertyAccess(PropertyAccess propertyAccess, TExpr self);
    
    protected abstract TExpr ObjectCreation(ObjectCreation objectCreation, List<TExpr> arguments);

    
    public TRes Transform(Project project)
    {
        // Transform the classes
        List<TClass> classes = project.Classes.Select(Transform).ToList();
        
        return Project(project, classes);
    }

    public TClass Transform(Class @class)
    {
        // Transform the methods
        List<TMethod> methods = @class.Methods.Select(Transform).ToList();
        
        // Transform the properties
        List<TProperty> properties = @class.Members.Select(Transform).ToList();
        
        return Class(@class, methods, properties);
    }
    
    public TMethod Transform(Method method)
    {
        // Transform the statements
        List<TStat> statements = method.Body.Select(Transform).ToList();
        
        return Method(method, statements);
    }
    
    public TProperty Transform(Property property)
    {
        // Transform the expressions
        TExpr? getter = property.Getter is null ? default : Transform(property.Getter);
        TExpr? setter = property.Setter is null ? default : Transform(property.Setter);
        TExpr? initializer = property.Initializer is null ? default : Transform(property.Initializer);
        
        return Property(property, getter, setter, initializer);
    }

    public TStat Transform(Statement statement)
    {
        return statement switch
        {
            Declaration declaration => Declaration(declaration, Transform(declaration.Expression)),
            Assignment assignment => Assignment(assignment, Transform(assignment.Expression)),
            Return @return => Return(@return, Transform(@return.ReturnExpr)),
            ExpressionStatement expressionStatement => ExpressionStatement(expressionStatement, Transform(expressionStatement.Expression)),
            If ifStatement => If(ifStatement, Transform(ifStatement.Condition), ifStatement.Then.Select(Transform), ifStatement.Else?.Select(Transform)),
            _ => throw new InvalidOperationException($"Unsupported statement {statement.GetType().Name}")
        };
    }

    public TExpr Transform(Expression expression)
    {
        return expression switch
        {
            Constant constant => Constant(constant),
            Variable variable => Variable(variable),
            FunctionCall functionCall => FunctionCall(functionCall, functionCall.Arguments.Select(Transform).ToList()),
            ListExpression listExpression => ListExpression(listExpression,
                listExpression.Members.Select(Transform).ToList()),
            ListAccessor listAccessor => ListAccessor(listAccessor, Transform(listAccessor.List),Transform(listAccessor.Accessor) ),
            MapAccessor mapAccessor => MapAccessor(mapAccessor, Transform(mapAccessor.Map), Transform(mapAccessor.Accessor)),
            Lambda lambda => Lambda(lambda, Transform(lambda.Body)),
            ObjectCreation objectCreation => ObjectCreation(objectCreation,
                objectCreation.Arguments.Select(Transform).ToList()),
            PropertyAccess propertyAccess => PropertyAccess(propertyAccess, Transform(propertyAccess.Self)),
            _ => throw new InvalidOperationException($"Unsupported expression {expression.GetType().Name}")
        };
    }

    protected abstract TExpr MapAccessor(MapAccessor mapAccessor, TExpr map, TExpr accessor);

    protected abstract TExpr ListAccessor(ListAccessor listAccessor, TExpr list, TExpr accessor);
}

public abstract record UnitProjectTransformer : ProjectTransformer<Project, Class, Method, Property, Statement, Expression>
{
    protected override Project Project(Project project, List<Class> classes)
    {
        return project with
        {
            Classes = classes
        };
    }

    protected override Class Class(Class @class, List<Method> methods, List<Property> properties)
    {
        return @class with
        {
            Methods = methods,
            Members = properties
        };
    }

    protected override Method Method(Method method, List<Statement> statements)
    {
        return method with
        {
            Body = statements.ToArray()
        };
    }
    
    protected override Property Property(Property property, Expression? getter, Expression? setter, Expression? initializer)
    {
        return property with
        {
            Getter = getter,
            Setter = setter,
            Initializer = initializer
        };
    }
    
    protected override Statement Declaration(Declaration declaration, Expression expression)
    {
        return declaration with
        {
            Expression = expression
        };
    }
    
    protected override Statement Assignment(Assignment assignment, Expression expression)
    {
        return assignment with
        {
            Expression = expression
        };
    }
    
    protected override Statement Return(Return returnStatement, Expression expression)
    {
        return returnStatement with
        {
            ReturnExpr = expression
        };
    }
    
    protected override Statement ExpressionStatement(ExpressionStatement expressionStatement, Expression expression)
    {
        return expressionStatement with
        {
            Expression = expression
        };
    }

    protected override Statement If(If ifStatement, Expression condition, IEnumerable<Statement> body, IEnumerable<Statement>? @else)
    {
        return ifStatement with
        {
            Cases = [(condition, body.ToList())],
            Default = @else?.ToList()
        };
    }

    protected override Expression Constant(Constant constant)
    {
        return constant;
    }
    
    protected override Expression Variable(Variable variable)
    {
        return variable;
    }
    
    protected override Expression FunctionCall(FunctionCall functionCall, List<Expression> arguments)
    {
        return functionCall with
        {
            Arguments = arguments
        };
    }
    
    protected override Expression ListExpression(ListExpression listExpression, List<Expression> members)
    {
        return listExpression with
        {
            Members = members
        };
    }

    protected override Expression ListAccessor(ListAccessor listAccessor, Expression list, Expression accessor)
    {
        return listAccessor with
        {
            Accessor = accessor,
            List = list
        };
    }

    protected override Expression MapAccessor(MapAccessor mapAccessor, Expression map, Expression accessor)
    {
        return mapAccessor with
        {
            Accessor = accessor,
            Map = map,
        };
    }
    
    protected override Expression Lambda(Lambda lambda, Expression body)
    {
        return lambda with
        {
            Body = body
        };
    }
    
    protected override Expression PropertyAccess(PropertyAccess propertyAccess, Expression self)
    {
        return propertyAccess with
        {
            Self = self
        };
    }
    
    protected override Expression ObjectCreation(ObjectCreation objectCreation, List<Expression> arguments)
    {
        return objectCreation with
        {
            Arguments = arguments
        };
    }
}


public abstract record BulkTransformer<TRes> : ProjectTransformer<TRes, TRes, TRes, TRes, TRes, TRes>
{
    protected abstract TRes Combine(IEnumerable<TRes> elements);
    
    protected override TRes Project(Project project, List<TRes> classes) => Combine(classes);

    protected override TRes Class(Class @class, List<TRes> methods, List<TRes> properties)
        => Combine(methods.Concat(properties));

    protected override TRes Method(Method method, List<TRes> statements) 
        => Combine(statements);

    protected override TRes Property(Property property, TRes? getter, TRes? setter, TRes? initializer)
        => Combine(new[] { getter, setter, initializer }.Where(x => x != null)!);

    protected override TRes Declaration(Declaration declaration, TRes expression)
    {
        return expression;
    }

    protected override TRes Assignment(Assignment assignment, TRes expression)
    {
        return expression;
    }

    protected override TRes Return(Return returnStatement, TRes expression)
    {
        return expression;
    }

    protected override TRes ExpressionStatement(ExpressionStatement expressionStatement, TRes expression)
    {
        return expression;
    }

    protected override TRes If(If ifStatement, TRes condition, IEnumerable<TRes> body, IEnumerable<TRes>? @else)
        => Combine(@else is null 
            ? body.Prepend(condition) 
            : body.Prepend(condition).Concat(@else));

    protected override TRes FunctionCall(FunctionCall functionCall, List<TRes> arguments)
        => Combine(arguments);

    protected override TRes ListExpression(ListExpression listExpression, List<TRes> members)
        => Combine(members);

    protected override TRes Lambda(Lambda lambda, TRes body)
    {
        return body;
    }

    protected override TRes PropertyAccess(PropertyAccess propertyAccess, TRes self)
    {
        return self;
    }

    protected override TRes ObjectCreation(ObjectCreation objectCreation, List<TRes> arguments)
        => Combine(arguments);

    protected override TRes MapAccessor(MapAccessor mapAccessor, TRes map, TRes accessor)
        => Combine(new[] { map, accessor });

    protected override TRes ListAccessor(ListAccessor listAccessor, TRes list, TRes accessor)
        => Combine(new[] { list, accessor });
}