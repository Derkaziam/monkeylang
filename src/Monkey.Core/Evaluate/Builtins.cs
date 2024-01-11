namespace Monk.Core.Evaluate;

public static class MonkeyBuiltins {
    public static readonly Dictionary<string, MonkeyBuiltin> Builtins = new() {
        { "len", new MonkeyBuiltin(Len) },
        { "first", new MonkeyBuiltin(First) },
        { "last", new MonkeyBuiltin(Last) },
        { "rest", new MonkeyBuiltin(Rest) },
        { "push", new MonkeyBuiltin(Push) },
        { "println", new MonkeyBuiltin(Println) },
    };

    private static IObject Len(List<IObject> args) {
        if (args.Count != 1)
            return new MonkeyError($"wrong number of arguments. got={args.Count}, want=1");
        if (args[0] is MonkeyString str)
            return new MonkeyInteger(str.Value.Length);
        if (args[0] is MonkeyArray arr)
            return new MonkeyInteger(arr.Elements.Count);
        return new MonkeyError($"argument to `len` not supported, got {args[0].Type()}");
    }

    private static IObject First(List<IObject> args) {
        if (args.Count != 1)
            return new MonkeyError($"wrong number of arguments. got={args.Count}, want=1");
        if (args[0] is MonkeyArray arr)
            return arr.Elements.Count > 0 ? arr.Elements[0] : new MonkeyNull();
        return new MonkeyError($"argument to `first` must be ARRAY, got {args[0].Type()}");
    }

    private static IObject Last(List<IObject> args) {
        if (args.Count != 1)
            return new MonkeyError($"Wrong number of arguments. Got={args.Count}, want=1");
        if (args[0] is not MonkeyArray arr)
            return new MonkeyError($"Argument to 'last' must be Array. Got {args[0].Type}");
        var length = arr.Elements.Count;
        return length > 0 ? arr.Elements[length - 1] : Evaluator.Null;
    }

    private static IObject Rest(List<IObject> args) {
        if (args.Count != 1)
            return new MonkeyError($"Wrong number of arguments. Got={args.Count}, want=1");
        if (args[0] is not MonkeyArray arr)
            return new MonkeyError($"Argument to 'last' must be Array. Got {args[0].Type}");
        var length = arr.Elements.Count;
        if (length > 0)
            return new MonkeyArray(arr.Elements.Skip(1).ToList());
        return Evaluator.Null;
    }

    private static IObject Push(List<IObject> args) {
        if (args.Count != 2)
            return new MonkeyError($"Wrong number of arguments. Got={args.Count}, want=2");
        if (args[0] is not MonkeyArray arr)
            return new MonkeyError($"Argument to 'push' must be Array. Got {args[0].Type}");
        var newElements = arr.Elements.Skip(0).ToList();
        newElements.Add(args[1]);
        return new MonkeyArray(newElements);
    }

    private static IObject Println(List<IObject> args) {
        foreach (var arg in args)
            Console.WriteLine(arg.Inspect());
        return Evaluator.Null;
    }
}
