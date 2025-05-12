using ExcelCompiler.Passes.Code;
using ExcelCompiler.Passes.Helpers;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Data;
using ExcelCompiler.Representations.Structure;
using Table = ExcelCompiler.Representations.Compute.Specialized.Table;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;
using Type = ExcelCompiler.Representations.CodeLayout.TopLevel.Type;

namespace ExcelCompiler.Passes;

[CompilerPass]
public class ComputeToCodePass
{
    private readonly ExtractTypesPass _extractTypesPass;
    
    public ComputeToCodePass(ExtractTypesPass extractTypesPass)
    {
        _extractTypesPass = extractTypesPass;
    }
    
    /// <summary>
    /// Transform the Compute Support Graph into a Code Layout, consolidating the Compute and Data layer.
    /// </summary>
    /// <param name="supportGraph">The support graph containing compute operations.</param>
    /// <param name="dataManager"></param>
    /// <returns></returns>
    public Project Transform(SupportGraph supportGraph, DataManager dataManager)
    {
        List<Class> output = [];
        
        List<Class> types = _extractTypesPass.ExtractTypes(supportGraph.Tables);
        output.AddRange(types);
        
        List<Statement> tableVariables = GenerateTableVars(supportGraph, dataManager, types);
        
        Statement[] body = tableVariables.Concat(GenerateStatements(supportGraph)).ToArray();
        Method main = new Method("Main", [], body);
        Class program = new Class("Program", [], [main]);
        
        output.Add(program);

        Project project = new()
        {
            Name = "Program",
            Classes = output,
        };
        
        return project;
    }

    
    private string VariableName(ComputeUnit cell) 
        => VariableName(cell.Location);
    
    private string VariableName(Location location)
        => (location.Spreadsheet + location.ToA1()).ToCamelCase();
    
    private Statement[] GenerateStatements(SupportGraph graph)
    {
        List<Statement> statements = new List<Statement>();
        
        // Create a variable per cell and start at the roots
        foreach (var cell in graph.EntryPointsOfCells().Reverse())
        {
            string varName = VariableName(cell);
            Type type = cell.Type is null ? Type.Derived : new Type(cell.Type);
            statements.Add(new Declaration(new Variable(varName.ToCamelCase(), type), Generate(cell)));
        }
        
        // Return all the roots
        // Right now, we only support a single root
        var root = graph.Roots.Single();
        Type rootType = new Type("double");
        statements.Add(new Return(new Variable(VariableName(root), rootType)));
        
        return statements.ToArray();
    }

    private Expression Generate(ComputeUnit cell)
    {
        return cell switch
        {
            ConstantValue<double> @double => new Constant(new Type("double"), @double.Value),
            
            Function {Name: "SUM", Dependencies: [RangeReference or TableReference]} func => new FunctionCall(Generate(func.Dependencies[0]), "Sum", []),
            
            Function func => new FunctionCall(func.Name, func.Dependencies.Select(Generate).ToList()),
            CellReference cellRef => new Variable(VariableName(cell.Dependencies[0]), Type.Derived),
            RangeReference range => new ListExpression(range.Reference.GetLocations().Select(l => new Variable(VariableName(l))).ToList<Expression>(), new Type("double")),
            TableReference tableRef => new FunctionCall(
                new Variable(tableRef.Reference.TableName.ToCamelCase()), 
                "Select", 
                [new Lambda([new Variable("t")], 
                    new PropertyAccess(Type.Derived, new Variable("t"), tableRef.Reference.ColumnName.ToPascalCase()))]),
            _ => throw new InvalidOperationException($"Unsupported compute unit {cell.GetType().Name}"),
        };
    }

    private List<Statement> GenerateTableVars(SupportGraph supportGraph, DataManager dataManager, List<Class> classes)
    {
        List<Statement> statements = new List<Statement>();

        foreach (Table table in supportGraph.Tables)
        {
            Class? type = classes.FirstOrDefault(c => c.Name == (table.Name + " Item").ToPascalCase());
            
            if (type is null) throw new InvalidOperationException($"Could not find class for table {table.Name}");

            Variable variable = new Variable(table.Name.ToCamelCase(), new ListOf(type));

            Expression expression = GenerateDataExpression(type, dataManager.Repositories.Single(r => r.Name == table.Name));
            Declaration variableDeclaration = new Declaration(variable, expression);
            
            statements.Add(variableDeclaration);
        }

        return statements;
    }

    private Expression GenerateDataExpression(Class type, IDataRepository single)
    {
        InMemoryDataRepository repo =  (single as InMemoryDataRepository)!;
        ColumnarDataSchema schema = (repo.Schema as ColumnarDataSchema)!;
        
        // Get columns needed for creating the type
        List<Expression> expressions = new List<Expression>();
        Method constructor = type.GenerateConstructor();

        Dictionary<string, int> argumentMap = constructor.Parameters.Select(p =>
        {
            // Get the index of the parameter in the row from the schema
            int index = schema.Properties.Select(x => x.Key.ToCamelCase()).ToList().IndexOf(p.Name);

            if (index == -1) throw new InvalidOperationException($"Could not find property {p.Name} in schema");

            return (p.Name, index);
        }).ToDictionary();
        
        foreach (var row in repo.GetDataFromRows())
        {
            List<Expression> arguments = constructor.Parameters.Select(p =>
            {
                // Get the type
                Type valueType = new Type(p.Type.Name);
                object value = row[argumentMap[p.Name]];
                
                // Get the value
                return new Constant(valueType, value);
            }).Cast<Expression>().ToList();
            
            
            // List<Expression> arguments = schema.Properties.Values.Zip(row)
            //     .Select((unit) =>
            //     {
            //         Type valueType = new Type(unit.First);
            //         return new Constant(valueType, unit.Second);
            //     }).Cast<Expression>().ToList();
            
            // Create new constructors
            ObjectCreation objectCreation = new ObjectCreation(type, arguments);
            
            expressions.Add(objectCreation);
        }

        return new ListExpression(expressions);
    }
}