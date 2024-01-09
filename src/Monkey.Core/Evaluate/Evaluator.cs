using System.Security.AccessControl;
using Monk.Core.Parse;

namespace Monk.Core.Evaluate;

public static class Evaluator {
    public static readonly MonkeyBoolean True = new(true);
    public static readonly MonkeyBoolean False = new(false);
    public static readonly MonkeyNull Null = new();
    public static IObject Eval(INode node) {
        switch (node) {
            // ********** //
            // Statements //
            // ********** //
            case ProgramNode p:
                return EvalProgram(p.Statements);
            case ExpressionStatement es:
                return Eval(es.Expression);
            
            // *********** //
            // Expressions //
            // *********** //
            case IntegerLiteral il:
                return new MonkeyInteger(il.Value);
            case BooleanLiteral bl:
                return new MonkeyBoolean(bl.Value);
            case PrefixExpression pe: {
                    var right = Eval(pe.Right);
                    return IsError(right) ? right : EvalPrefixExpression(pe.Operator, right);
                }
            case InfixExpression ie: {
                    var left = Eval(ie.Left);
                    if (IsError(left)) return left;
                    var right = Eval(ie.Right);
                    return IsError(right) ? right : EvalInfixExpression(ie.Operator, left, right);
                }
            
            default:
                throw new Exception($"Invalid node type: {node.GetType()}");
        }

    }

    // ******* //
    // Tooling //
    // ******* //
    private static bool IsError(IObject obj) => obj.Type() == ObjectType.ERROR;
    private static MonkeyBoolean NativeBoolToBooleanObject(bool value) => value ? True : False;

    // ********** //
    // Evaluation //
    // ********** //
    private static IObject EvalProgram(List<IStatement> statements) {
        IObject result = Null;

        foreach (IStatement s in statements) result = Eval(s);

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
        if (right.Type() != ObjectType.INTEGER) return Null;
        var value = ((MonkeyInteger)right).Value;
        return new MonkeyInteger(-value);
    }

    private static IObject EvalInfixExpression(string op, IObject left, IObject right) {
        if (left.Type() == ObjectType.INTEGER && right.Type() == ObjectType.INTEGER)
            return EvalIntegerInfixExpression(op, left, right);
        if (op == "==")
            return NativeBoolToBooleanObject(((MonkeyBoolean)left).Value == ((MonkeyBoolean)right).Value);
        if (op == "!=")
            return NativeBoolToBooleanObject(((MonkeyBoolean)left).Value != ((MonkeyBoolean)right).Value);
        return Null;
    }

    private static IObject EvalIntegerInfixExpression(string op, IObject left, IObject right) {
        var leftVal = ((MonkeyInteger)left).Value;
        var rightVal = ((MonkeyInteger)right).Value;

        return op switch {
            "+" => new MonkeyInteger(leftVal + rightVal),
            "-" => new MonkeyInteger(leftVal - rightVal),
            "*" => new MonkeyInteger(leftVal * rightVal),
            "/" => new MonkeyInteger(leftVal / rightVal),
            "<" => NativeBoolToBooleanObject(leftVal < rightVal),
            ">" => NativeBoolToBooleanObject(leftVal > rightVal),
            "==" => NativeBoolToBooleanObject(leftVal == rightVal),
            "!=" => NativeBoolToBooleanObject(leftVal != rightVal),
            _ => Null,
        };
    }
}
