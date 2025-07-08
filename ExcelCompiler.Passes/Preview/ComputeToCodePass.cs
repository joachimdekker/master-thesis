using ExcelCompiler.Passes.Code;
using ExcelCompiler.Passes.Helpers;
using ExcelCompiler.Passes.Preview.Code;
using ExcelCompiler.Representations.CodeLayout;
using ExcelCompiler.Representations.CodeLayout.Expressions;
using ExcelCompiler.Representations.CodeLayout.Statements;
using ExcelCompiler.Representations.CodeLayout.TopLevel;
using ExcelCompiler.Representations.Compute;
using ExcelCompiler.Representations.Compute.Specialized;
using ExcelCompiler.Representations.Data.Preview.Specialized;
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

    private DataManager _dataManager;
    private ComputeGraph _computeGraph;
    
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

        // Set the global variables (yeah yeah I know, this is a big no no but please, I just want it to work)
        _dataManager = dataManager;
        _computeGraph = computeGraph;
        
        var types = _typeGenerator.Generate(computeGraph, dataManager);
        output.AddRange(types);

        var constructVariables = PopulateConstructClasses(computeGraph, dataManager, types);
        //
        // var tableVariables = GenerateTableVars(computeGraph, dataManager, types);

        Variable[] parameters = computeGraph.Inputs.Select(i => new Variable(VariableName(i.Location), new Type(i.Type))).ToArray();
        var body = constructVariables.Concat(GenerateStatements(computeGraph)).Where(s => s is not Declaration a || parameters.All(p => p.Name != a.Variable.Name)).ToArray();

        
        var main = new Method("Main", parameters, body);
        var program = new Class("Program", [], [main]);

        output.Add(program);

        Project project = new()
        {
            Name = "Program",
            Classes = output,
        };

        return project;
    }

    private IEnumerable<Statement> PopulateConstructClasses(ComputeGraph computeGraph, DataManager dataManager, List<Class> types)
    {
        List<Statement> statements = new List<Statement>();
        foreach (Construct construct in computeGraph.Constructs)
        {
            Class type;
            IEnumerable<Statement> initializer;
            switch (construct)
            {
                case Table table:
                    type = types.First(t => t.Name == (table.Name + " Item").ToPascalCase());
                    initializer = PopulateTableClass(table, 
                        type,
                        (dataManager[table.Name] as TableRepository)!,
                        computeGraph);
                    statements.AddRange(initializer);
                    break;
                case Chain chain:
                    type = types.First(t => t.Name == chain.Name.ToPascalCase());
                    initializer = PopulateChainClass(chain, 
                        type,
                        (dataManager[chain.Name] as ChainRepository)!,
                        computeGraph);
                    statements.AddRange(initializer);
                    break;
                
                default:
                    throw new InvalidOperationException($"Unsupported construct {construct.GetType().Name}");
            }
        }
        
        return statements;
    }

    private IEnumerable<Statement> PopulateChainClass(Chain chain, Class type, ChainRepository chainRepository, ComputeGraph computeGraph)
    {
        // Create a new chain class
        // The chain class is actually just the type with a constructor as a list of lists.

        Method constructor = type.GenerateConstructor();
        
        IEnumerable<Expression> arguments = 
            from parameter in constructor.Parameters 
            let value = chainRepository.Data.Single(x => x.Key.ToCamelCase() == parameter.Name).Value
            select Constant.List(parameter.Type as ListOf, value);
        
        Expression creation = new ObjectCreation(type, arguments.ToList());
        Variable variable = new Variable(chain.Name.ToCamelCase(), type);
        
        return [new Declaration(variable, creation)];
    }

    private IEnumerable<Statement> PopulateTableClass(Table table, Class type, TableRepository tableRepo, ComputeGraph computeGraph)
    {
        List<Statement> statements = new List<Statement>();
        
        Method constructor = type.GenerateConstructor();

        List<Expression> expressions = new List<Expression>();
        foreach (var row in tableRepo.GetRows())
        {
            List<Expression> arguments = constructor.Parameters.Select(p =>
            {
                // Get the type
                Type valueType = new Type(p.Type.Name);
                object value = row.Single(x => x.Key.ToCamelCase() == p.Name).Value;

                // Get the value
                return new Constant(valueType, value);
            }).Cast<Expression>().ToList();

            // Create new constructors
            ObjectCreation objectCreation = new ObjectCreation(type, arguments);

            expressions.Add(objectCreation);
        }

        // Construct the class
        // For the table, it is just a list of the items
        Expression creation = new ListExpression(expressions);
        Variable variable = new Variable(table.Name.ToCamelCase(), new ListOf(type));
        
        return [new Declaration(variable, creation)];
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
            statements.Add(new Declaration(new Variable(varName.ToCamelCase(), new Type(cell.Type ?? typeof(double))), Generate(cell)));
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

            Function {Name: "SUM", Dependencies: [RangeReference or TableReference or ColumnOperation]} func => new FunctionCall(Generate(func.Dependencies[0]), "Sum", []),
            
            Function func => new FunctionCall(func.Name, func.Dependencies.Select(Generate).ToList()),
            
            CellReference cellRef => new Variable(VariableName(cellRef.Reference), new Type(cellRef.Type)),
            
            RangeReference range => new ListExpression(range.Dependencies.Select(l => new Variable(VariableName(l.Location))).ToList<Expression>(), new Type("double")),
            
            TableReference tableRef => new FunctionCall(
                new Variable(tableRef.Reference.TableName.ToCamelCase()),
                "Select",
                [new Lambda([new Variable("t")],
                    new PropertyAccess(Type.Derived, new Variable("t"), tableRef.Reference.ColumnNames[0].ToPascalCase()))]),
            
            RecursiveChainColumn.RecursiveCellReference recursiveCellReference => new FunctionCall(new Variable(recursiveCellReference.ChainName.ToCamelCase()), recursiveCellReference.ColumnName.ToPascalCase() + "At", [new Constant(recursiveCellReference.Recursion)]),
            
            ComputedChainColumn.CellReference computedChainColumn => new ListAccessor(
                new Type(computedChainColumn.Type), 
                new PropertyAccess(new ListOf(new Type(computedChainColumn.Type)), new Variable(computedChainColumn.ChainName.ToCamelCase()), computedChainColumn.ColumnName.ToCamelCase()), 
                new Constant(computedChainColumn.Index)),
            
            ColumnOperation { Structure: Table table } op => new FunctionCall(
                new Variable(table.Name.ToCamelCase()),
                "Select",
                [new Lambda([new Variable("t")],
                    new PropertyAccess(Type.Derived, new Variable("t"), op.ColumnName))]),
            
            ColumnOperation { Structure: Chain chain } op => new PropertyAccess(Type.Derived,
                new Variable(chain.Name.ToCamelCase()),
                op.ColumnName),
            
            DataChainColumn.Reference dataRef => new ListAccessor(
                new Type(dataRef.Type), 
            new PropertyAccess(new ListOf(new Type(dataRef.Type)), new Variable(dataRef.ChainName.ToCamelCase()), dataRef.ColumnName.ToPascalCase()), 
            new Constant(dataRef.Index)),
            
            Input input => new Variable(VariableName(input.Location)),
            
            _ => throw new InvalidOperationException($"Unsupported compute unit {cell.GetType()}"),
        };
    }
}
