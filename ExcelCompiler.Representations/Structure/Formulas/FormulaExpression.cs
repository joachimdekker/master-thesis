using Irony.Parsing;
using XLParser;

namespace ExcelCompiler.Representations.Structure.Formulas;

public record FormulaExpression()
{
    public static FormulaExpression Parse(string formula, FormulaContext context)
    {
        ParseTreeNode root = ExcelFormulaParser.Parse(formula);

        return Parse(root);

        FormulaExpression Parse(ParseTreeNode node)
        {
            node = node.SkipToRelevant();
            
            return node.Type() switch
            {
                GrammarNames.FunctionCall => ParseFunction(node),
                GrammarNames.Reference => ParseReference(node),
                GrammarNames.Constant => Parse(node.ChildNodes[0]),
                GrammarNames.Text => new Constant(node.ChildNodes[0].Token.ValueString),
                GrammarNames.Number => new Constant(double.Parse(node.ChildNodes[0].Token.ValueString)),
                _ => throw new InvalidOperationException("Node type is not supported.")
            };
        }

        Reference ParseReference(ParseTreeNode node)
        {
            Domain.Structure.Reference reference = Domain.Structure.Reference.Parse(node.Print(), spreadsheet: context.Spreadsheet);
            
            return Reference.Parse(reference);
        }

        Function ParseFunction(ParseTreeNode node)
        {
            if (node.IsBinaryOperation())
            {
                FormulaExpression left = Parse(node.ChildNodes[0]);
                FormulaExpression right = Parse(node.ChildNodes[2]);

                return node.ChildNodes[1].Token.ValueString switch
                {
                    "+" => Operator.Plus(left, right),
                    "-" => Operator.Minus(left, right),
                    "*" => Operator.Multiply(left, right),
                    "/" => Operator.Divide(left, right),
                    "^" => Operator.Power(left, right),
                    "%" => Operator.Modulo(left, right),
                    _ => throw new ArgumentException("Unsupported operator.", nameof(node))
                };
            }
        
            if (node.IsFunction())
            {
                // Excel function call
                IEnumerable<FormulaExpression> dependencies = node.GetFunctionArguments().Select(c => Parse(c));
                Function func = new Function(node.GetFunction(), dependencies.ToList());

                return func;
            }
        
            // It can also be a unary operator like a percentage or something
            if (node.IsUnaryPrefixOperation())
            {
                if (node.ChildNodes[0].Token.ValueString != "-") throw new ArgumentException($"Unsupported operator {node.ChildNodes[0].Token.ValueString}.", nameof(node));

                return Operator.Negate(Parse(node.ChildNodes[1]));
            }
        
            throw new ArgumentException("Unsupported parse tree node.", nameof(node));
        }
    }
};