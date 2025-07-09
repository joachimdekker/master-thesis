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
using ExcelCompiler.Representations.Helpers;
using ExcelCompiler.Representations.References;
using Table = ExcelCompiler.Representations.Compute.Specialized.Table;
using TableReference = ExcelCompiler.Representations.Compute.TableReference;
using Type = ExcelCompiler.Representations.CodeLayout.Type;
using DataManager = ExcelCompiler.Representations.Data.Preview.DataManager;
using IDataRepository = ExcelCompiler.Representations.Data.Preview.IDataRepository;
using ListOf = ExcelCompiler.Representations.CodeLayout.ListOf;

namespace ExcelCompiler.Passes.Preview;

[CompilerPass]
public class ComputeToCodePass
{
    private readonly GenerateTypes _typeGenerator;

    private DataManager _dataManager;
    private ComputeGraph _computeGraph;
    private List<Class> _classes;
    
    public ComputeToCodePass(GenerateTypes typeGenerator)
    {
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
        _classes = types;
        output.AddRange(types);

        // var constructVariables = PopulateConstructClasses(computeGraph, dataManager, types);
        //
        // var tableVariables = GenerateTableVars(computeGraph, dataManager, types);

        List<Input> inputs = computeGraph.Inputs.ToList();
        Variable[] parameters = inputs.Select(i => new Variable(VariableName(i.Location), i.Type.Convert())).ToArray();
        // var body = constructVariables.Concat(GenerateStatements(computeGraph)).Where(s => s is not Declaration a || parameters.All(p => p.Name != a.Variable.Name)).ToArray();
        var body = GenerateStatements(computeGraph)
            .Where(s => s is not Declaration a || parameters.All(p => p.Name != a.Variable.Name))
            .ToArray();
        
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

    private Expression CreateChainConstructor(Class type)
    {
        Method constructor = type.GenerateConstructor();
        
        Chain? construct = _computeGraph.Constructs.Single(c => c.Id.ToPascalCase() == type.Name) as Chain;
        
        IEnumerable<Expression> arguments = 
            from parameter in constructor.Parameters 
            let column = construct.StructureData.Columns.Single(x => x.ColumnId.ToCamelCase() == parameter.Name)
            let values = column.Data
            let expressions = 
                values.Select<ComputeUnit, Expression>(value => value is Nil
                    ? new Constant(column.Type.Convert(), column.Type.DefaultValue)
                    : new Variable(VariableName(value), column.Type.Convert()))
            select new ListExpression(expressions.ToList());
        
        Expression creation = new ObjectCreation(type, arguments.ToList());
        return creation;
    }

    private Expression CreateTableConstructor(Class type)
    {
        Method constructor = type.GenerateConstructor();
        Table? construct = _computeGraph.Constructs.Single(c => c.Id.ToPascalCase() + "Item" == type.Name) as Table;

        List<Expression> newExpressions = [];
        for (int i = 0; i < construct!.StructureData!.Columns[0].Data.Count; i++)
        {
            var arguments = constructor.Parameters.Select<Variable,Expression>(p =>
            {
                // Get the type
                var values = construct!.StructureData!.Columns.Single(x => x.ColumnId.ToCamelCase() == p.Name);
                
                // Get the value
                var value = values.Data[i];

                if (value is Nil)
                {
                    return new Constant(values.Type.Convert(), values.Type.DefaultValue);
                }
                
                return new Variable(VariableName(value), values.Type.Convert());
            }).ToList();
            var item = new ObjectCreation(type, arguments);
            newExpressions.Add(item);
        }

        // Construct the class
        // For the table, it is just a list of the items
        Expression creation = new ListExpression(newExpressions);
        return creation;
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
            if (cell is ConstructCreation cc)
            {
                //Construct construct = _computeGraph.Constructs.Single(c => c.Id == cc.ConstructId);
                string constructName = cc.ConstructId.ToCamelCase();
                var expression = CreateConstruct(cc);
                Type type = expression.Type; 
                statements.Add(new Declaration(new Variable(constructName, type), expression));
                continue;
            }
            
            string varName = VariableName(cell);
            var experssion = Generate(cell);
            statements.Add(new Declaration(new Variable(varName.ToCamelCase(), cell.Type.Convert()), Generate(cell)));
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
            
            CellReference cellRef => new Variable(VariableName(cellRef.Reference), cellRef.Type.Convert()),
            
            RangeReference range => new ListExpression(range.Dependencies.Select(l => new Variable(VariableName(l.Location))).ToList<Expression>(), new Type("double")),
            
            TableReference tableRef => new FunctionCall(
                new Variable(tableRef.Reference.TableName.ToCamelCase()),
                "Select",
                [new Lambda([new Variable("t")],
                    new PropertyAccess(Type.Derived, new Variable("t"), tableRef.Reference.ColumnNames[0].ToPascalCase()))]),
            
            RecursiveChainColumn.RecursiveCellReference recursiveCellReference => new FunctionCall(new Variable(recursiveCellReference.ChainName.ToCamelCase()), recursiveCellReference.ColumnName.ToPascalCase() + "At", [new Constant(recursiveCellReference.Recursion)]),
            RecursiveResultReference recursiveCellReference => new FunctionCall(new Variable(recursiveCellReference.StructureName.ToCamelCase()), recursiveCellReference.ColumnName.ToPascalCase() + "At", [new Constant(recursiveCellReference.Row)]),
            
            ComputedChainColumn.CellReference computedChainColumn => new ListAccessor(
                computedChainColumn.Type.Convert(), 
                new PropertyAccess(new ListOf(computedChainColumn.Type.Convert()), new Variable(computedChainColumn.ChainName.ToCamelCase()), computedChainColumn.ColumnName.ToCamelCase()), 
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
                dataRef.Type.Convert(), 
            new PropertyAccess(new ListOf(dataRef.Type.Convert()), new Variable(dataRef.ChainName.ToCamelCase()), dataRef.ColumnName.ToPascalCase()), 
            new Constant(dataRef.Index)),
            
            Input input => new Variable(VariableName(input.Location)),
            
            TableColumn.CellReference cr => new Variable(cr.TableName + cr.ColumnName + cr.Index),
            
            ConstructCreation cc => CreateConstruct(cc),
            
            _ => throw new InvalidOperationException($"Unsupported compute unit {cell.GetType()}"),
        };
    }

    private Expression CreateConstruct(ConstructCreation cc)
    {
        // Get the construct
        Construct construct = _computeGraph.Constructs.Single(c => c.Id == cc.ConstructId);
        Class type = _classes.Single(c => c.Name.Contains(cc.ConstructId.ToPascalCase()));

        return construct switch
        {
            Table t => CreateTableConstructor(type),
            Chain c => CreateChainConstructor(type),
            _ => throw new InvalidOperationException($"Unsupported construct {construct.GetType().Name}")
        };
    }
}
