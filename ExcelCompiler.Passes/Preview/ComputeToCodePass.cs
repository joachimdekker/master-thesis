using ExcelCompiler.Passes.Code;
using ExcelCompiler.Passes.Helpers;
using ExcelCompiler.Passes.Preview.Code;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.References;
using Table = ExcelCompiler.Representations.Compute.Specialized.Table;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;
using Type = ExcelCompiler.Representations.CodeLayout.Type;
using DataManager = ExcelCompiler.Representations.Data.Preview.DataManager;
using IDataRepository = ExcelCompiler.Representations.Data.Preview.IDataRepository;

namespace ExcelCompiler.Passes.Preview;

[CompilerPass]
public class ComputeToCodePass
{
    private readonly ExtractDataClasses _extractDataClasses;
    private readonly GenerateTypes _typeGenerator;

    public ComputeToCodePass(ExtractDataClasses extractDataClasses, GenerateTypes typeGenerator)
    {
        _extractDataClasses = extractDataClasses;
        _typeGenerator = typeGenerator;
    }

    /// <summary>
    /// Transform the Compute Support Graph into a Code Layout, consolidating the Compute and Data layer.
    /// </summary>
    /// <param name="computeGraph">The support graph containing compute operations.</param>
    /// <param name="dataManager"></param>
    /// <returns></returns>
    public Project Transform(ComputeGraph computeGraph, DataManager dataManager)
    {
        List<Class> output = [];

        var types = _typeGenerator.Generate(computeGraph, dataManager);
        output.AddRange(types);

        var tableVariables = GenerateTableVars(computeGraph, dataManager, types);

        var body = tableVariables.Concat(GenerateStatements(computeGraph)).ToArray();
        var main = new Method("Main", [], body);
        var program = new Class("Program", [], [main]);

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

    private Statement[] GenerateStatements(ComputeGraph graph)
    {
        List<Statement> statements = [];

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
            CellReference cellRef => new Variable(VariableName(cellRef.Reference), new Type(cellRef.Type)),
            RangeReference range => new ListExpression(range.Dependencies.Select(l => new Variable(VariableName(l.Location))).ToList<Expression>(), new Type("double")),
            TableReference tableRef => new FunctionCall(
                new Variable(tableRef.Reference.TableName.ToCamelCase()),
                "Select",
                [new Lambda([new Variable("t")],
                    new PropertyAccess(Type.Derived, new Variable("t"), tableRef.Reference.ColumnNames[0].ToPascalCase()))]),
            _ => throw new InvalidOperationException($"Unsupported compute unit {cell.GetType().Name}"),
        };
    }

    private List<Statement> GenerateTableVars(ComputeGraph computeGraph, DataManager dataManager, List<Class> classes)
    {
        List<Statement> statements = new List<Statement>();

        foreach (Table table in computeGraph.Constructs)
        {
            Class? type = classes.FirstOrDefault(c => c.Name == (table.Name + " Item").ToPascalCase());

            if (type is null) throw new InvalidOperationException($"Could not find class for table {table.Name}");

            Variable variable = new Variable(table.Name.ToCamelCase(), new ListOf(type));

            Expression expression = GenerateDataExpression(type, dataManager[table.Name]);
            Declaration variableDeclaration = new Declaration(variable, expression);

            statements.Add(variableDeclaration);
        }

        return statements;
    }

    private Expression GenerateDataExpression(Class type, IDataRepository single)
    {
        Representations.Data.Preview.InMemoryDataRepository repo =  (single as Representations.Data.Preview.InMemoryDataRepository )!;
        Representations.Data.Preview.DataSchema schema = (repo.Schema as Representations.Data.Preview.DataSchema)!;

        // Get columns needed for creating the type
        List<Expression> expressions = new List<Expression>();
        Method constructor = type.GenerateConstructor();

        Dictionary<string, int> argumentMap = constructor.Parameters.Select(p =>
        {
            // Get the index of the parameter in the row from the schema
            int index = schema.Types.Select(x => x.Key.ToCamelCase()).ToList().IndexOf(p.Name);

            if (index == -1) throw new InvalidOperationException($"Could not find property {p.Name} in schema");

            return (p.Name, index);
        }).ToDictionary();

        foreach (var row in repo.GetRows())
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
