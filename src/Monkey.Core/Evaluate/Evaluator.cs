using Monk.Core.Parse;

namespace Monk.Core.Evaluate;

public static class Evaluator {
    public static readonly MonkeyBoolean True = new(true);
    public static readonly MonkeyBoolean False = new(false);
    public static readonly MonkeyNull Null = new();
    public static IObject Eval(INode node, MonkeyEnvironment env) {
        switch (node) {
            // ********** //
            // Statements //
            // ********** //
            case ProgramNode p:
                return EvalProgram(p.Statements, env);
            case ExpressionStatement es:
                return Eval(es.Expression, env);
            case BlockStatement bs:
                return EvalBlockStatement(bs.Statements, env);
            case ReturnStatement rs: {
                    IObject value = Eval(rs.ReturnValue, env);
                    return IsError(value) ? value : new MonkeyReturnValue(value);
                }
            case LetStatement ls: {
                IObject val = Eval(ls.Value, env);
                return IsError(val) ? val : env.Set(ls.Name.Value, val);
                }
            
            // *********** //
            // Expressions //
            // *********** //
            case IntegerLiteral il:
                return new MonkeyInteger(il.Value);
            case BooleanLiteral bl:
                return NativeBoolToBooleanObject(bl.Value);
            case StringLiteral sl:
                return new MonkeyString(sl.Value);
            case FunctionLiteral fl:
                List<Identifier> param = fl.Parameters;
                BlockStatement body = fl.Body;
                return new MonkeyFunction(param, body, env);
            case ArrayLiteral al:
                var elements = EvalExpressions(al.Elements, env);
                if (elements.Count == 1 && IsError(elements[0]))
                    return elements[0];
                return new MonkeyArray(elements);
            case Identifier ident:
                return EvalIdentifier(ident, env);
            case IfExpression ife:
                return EvalIfExpression(ife, env);
            case CallExpression ce: {
                var function = Eval(ce.Function, env);
                if (IsError(function))
                    return function;

                var args = EvalExpressions(ce.Arguments, env);
                if (args.Count == 1 && IsError(args[0]))
                    return args[0];
                return ApplyFunction(function, args);
            }
            case IndexExpression ie: {
                IObject left = Eval(ie.Left, env);
                if (IsError(left)) return left;
                IObject index = Eval(ie.Index, env);
                if (IsError(index)) return index;
                return EvalIndexExpression(left, index, env);

            }
            case PrefixExpression pe: {
                    var right = Eval(pe.Right, env);
                    return IsError(right) ? right : EvalPrefixExpression(pe.Operator, right);
                }
            case InfixExpression ie: {
                    var left = Eval(ie.Left, env);
                    if (IsError(left)) return left;
                    var right = Eval(ie.Right, env);
                    return IsError(right) ? right : EvalInfixExpression(ie.Operator, left, right);
                }
            
            default:
                return new MonkeyError($"unknown node {node}");
        }
    }

    // ******* //
    // Tooling //
    // ******* //
    private static bool IsError(IObject obj) => obj.Type() == ObjectType.ERROR;
    private static MonkeyBoolean NativeBoolToBooleanObject(bool value) => value ? True : False;
    private static bool IsTrue(IObject obj) {
        if (obj.Type() == ObjectType.NULL) {
            return false;
        } else if (obj.Type() == ObjectType.BOOLEAN) {
            return ((MonkeyBoolean)obj).Value;
        }
        return true;
    }

    // ********** //
    // Evaluation //
    // ********** //
    private static IObject EvalProgram(List<IStatement> statements, MonkeyEnvironment env) {
        IObject result = Null;

        foreach (IStatement s in statements) {
            result = Eval(s, env);

            if (result is MonkeyReturnValue returnValue) return returnValue.Value;
            if (result is MonkeyError error) return error;
        }

        return result;
    }

    private static IObject EvalBlockStatement(List<IStatement> bs, MonkeyEnvironment env) {
        IObject result = Null;

        foreach (IStatement s in bs) {
            result = Eval(s, env);
            var returnType = result.Type();

            if (returnType is ObjectType.RETURN_VALUE or ObjectType.ERROR) return result;
        }

        return result;
    }

    private static IObject EvalPrefixExpression(string op, IObject right) {
        return op switch {
            "!" => EvalBangOperatorExpression(right),
            "-" => EvalMinusPrefixOperatorExpression(right),
            _ => new MonkeyError($"unknown operator: {op}{right.Type()}")
        };
    }

    private static IObject EvalBangOperatorExpression(IObject right) {
        if (ReferenceEquals(right, True)) return False;
        if (ReferenceEquals(right, False)) return True;
        if (ReferenceEquals(right, Null)) return True;
        return False;
    }

    private static IObject EvalMinusPrefixOperatorExpression(IObject right) {
        if (right.Type() != ObjectType.INTEGER) return new MonkeyError($"unknown operator: -{right.Type()}");
        var value = ((MonkeyInteger)right).Value;
        return new MonkeyInteger(-value);
    }

    private static IObject EvalInfixExpression(string op, IObject left, IObject right) {
        if (left.Type() == ObjectType.INTEGER && right.Type() == ObjectType.INTEGER)
            return EvalIntegerInfixExpression(op, left, right);
        if (left.Type() == ObjectType.STRING && right.Type() == ObjectType.STRING)
            return EvalStringInfixExpression(op, left, right);
        if (op == "==")
            return NativeBoolToBooleanObject(left == right);
        if (op == "!=")
            return NativeBoolToBooleanObject(left != right);
        return new MonkeyError($"unknown operator: {left.Type()} {op} {right.Type()}");
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
            _ => new MonkeyError($"unknown operator: {left.Type()} {op} {right.Type()}"),
        };
    }

    private static IObject EvalStringInfixExpression(string op, IObject left, IObject right) {
        if (op != "+") return new MonkeyError($"unknown operator: {left.Type()} {op} {right.Type()}");
        
        var leftVal = ((MonkeyString)left).Value;
        var rightVal = ((MonkeyString)right).Value;
        return new MonkeyString(leftVal + rightVal);
    }

    private static IObject EvalIfExpression(IfExpression ie, MonkeyEnvironment env) {
        IObject condition = Eval(ie.Condition, env);
        if (IsError(condition)) return condition;

        if (IsTrue(condition)) {
            return Eval(ie.Consequence, env);
        } else if (ie.Alternative != null) {
            return Eval(ie.Alternative, env);
        } else {
            return Null;
        }   
    }
    
    private static IObject EvalIdentifier(Identifier id, MonkeyEnvironment env) {
        var (val, inCurrentEnvironment) = env.Get(id.Value);
        if (inCurrentEnvironment)
            return val!;

        var inBuiltinEnvironment = MonkeyBuiltins.Builtins.TryGetValue(id.Value, out var fn);
        if (inBuiltinEnvironment)
            return fn!;
        return new MonkeyError($"Identifier not found: {id.Value}");
    }

    private static List<IObject> EvalExpressions(List<IExpression> exprs, MonkeyEnvironment env) {
        List<IObject> result = new();

        foreach (IExpression e in exprs) {
            IObject evaluated = Eval(e, env);
            if (IsError(evaluated)) return new List<IObject> { evaluated };
            result.Add(evaluated);
        }

        return result;
    }

    private static IObject EvalIndexExpression(IObject left, IObject index, MonkeyEnvironment env) {
        return left.Type() switch {
            ObjectType.ARRAY when index.Type() == ObjectType.INTEGER => EvalArrayIndexExpression(left, index),
            _ => new MonkeyError($"Index operator not supported {left.Type}")
        };
    }

    private static IObject EvalArrayIndexExpression(IObject array, IObject index) {
        var arrayObject = (MonkeyArray)array;
        long idx = ((MonkeyInteger)index).Value;
        long max = arrayObject.Elements.Count - 1;

        if (idx < 0 || idx > max) {
            return new MonkeyError("index out of bounds");
        }
        return (IObject)arrayObject.Elements[(int)idx];
    }

    private static IObject ApplyFunction(IObject fn, List<IObject> args) {
        if (fn is MonkeyFunction f) {
            var extendedEnv = ExtendFunctionEnv(f, args);
            var evaluated = Eval(f.Body, extendedEnv);
            return evaluated;
        }
        if (fn is MonkeyBuiltin b) return b.Fn(args);
        return new MonkeyError($"not a function: {fn.Type()}");
    }

    private static MonkeyEnvironment ExtendFunctionEnv(MonkeyFunction fn, IReadOnlyList<IObject> args) {
        var env = MonkeyEnvironment.NewEnclosedEnvironment(fn.Env);
        for (var i = 0; i < fn.Parameters.Count; i++)
            env.Set(fn.Parameters[i].Value, args[i]);
        return env;
    }

    private static IObject UnwrapReturnValue(IObject obj) {
        if (obj is MonkeyReturnValue rv) return rv.Value;
        return obj;
    }
}
