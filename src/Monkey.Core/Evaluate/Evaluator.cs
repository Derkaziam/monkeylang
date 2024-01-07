using System.Security.AccessControl;
using Monk.Core.Parse;

namespace Monk.Core.Evaluate;

public static class Evaluator {
    public static readonly MonkeyBoolean True = new(true);
    public static readonly MonkeyBoolean False = new(false);
    public static readonly MonkeyNull Null = new();
    public static IObject Eval(INode node) {
        switch (node) {
            case ProgramNode p:
                return EvalProgram(p.Statements);
            case ExpressionStatement es:
                return Eval(es);
            case IntegerLiteral il:
                return new MonkeyInteger(il.Value);
            case BooleanLiteral bl:
                return new MonkeyBoolean(bl.Value);
            case PrefixExpression pe:
                var right = Eval(pe.Right);
                return IsError(right) ? right : EvalPrefixExpression(pe.Operator, right);
            default:
                throw new Exception($"Invalid node type: {node.GetType()}");
        }

    }

    // ******* //
    // Tooling //
    // ******* //
    private static bool IsError(IObject obj) => obj.Type == ObjectType.ERROR;

    // ********** //
    // Evaluation //
    // ********** //
    private static IObject EvalProgram(List<IStatement> statements) {
        IObject result = Null;

        foreach (var s in statements) result = Eval(s);

        return result;
    }

    private static IObject EvalPrefixExpression(string op, IObject right) {
        return op switch {
            "!" => EvalBangOperatorExpression(right),
            "-" => EvalMinusPrefixOperatorExpression(right),
            _ => Null
        };
    }

    private static IObject EvalBangOperatorExpression(IObject right) {
        if (ReferenceEquals(right, True)) return False;
        if (ReferenceEquals(right, False)) return True;
        if (ReferenceEquals(right, Null)) return True;
        return False;
    }

    private static IObject EvalMinusPrefixOperatorExpression(IObject right) {
        if (right.Type != ObjectType.INTEGER) return Null;
        var value = ((MonkeyInteger)right).Value;
        return new MonkeyInteger(-value);
    }
}
