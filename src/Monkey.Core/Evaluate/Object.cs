using Monk.Core.Parse;

namespace Monk.Core.Evaluate;

using BuiltinFunction = Func<List<IObject>, IObject>;
public enum ObjectType {
    NONE,
    ERROR,
    RETURN_VALUE,
    
    INTEGER,
    BOOLEAN,
    STRING,
    NULL,

    ARRAY,

    FUNCTION,
    BUILTIN_FUNCTION,
}

public interface IObject {
    ObjectType Type();
    string Inspect();
}

public record MonkeyInteger(long Value) : IObject {
    public ObjectType Type() => ObjectType.INTEGER;
    public string Inspect() => $"{Value}"; 
}

public record MonkeyBoolean(bool Value) : IObject {
    public ObjectType Type() => ObjectType.BOOLEAN;
    public string Inspect() => $"{Value}"; 
}
public record MonkeyString(string Value) : IObject {
    public ObjectType Type() => ObjectType.STRING;
    public string Inspect() => $"{Value}"; 
}

public record MonkeyNull : IObject {
    public ObjectType Type() => ObjectType.NULL;
    public string Inspect() => $"null"; 
}

public record MonkeyReturnValue(IObject Value) : IObject {
    public ObjectType Type() => ObjectType.RETURN_VALUE;
    public string Inspect() => Value.Inspect();
}

public record MonkeyError(string Message) : IObject {
    public ObjectType Type() => ObjectType.ERROR;
    public string Inspect() => $"Error: {Message}";
}

public record MonkeyFunction(List<Identifier> Parameters, BlockStatement Body, MonkeyEnvironment Env) : IObject {
    public ObjectType Type() => ObjectType.FUNCTION;

    public string Inspect() {
        var parameters = Parameters.Select(p => p);
        return $"fn({string.Join(", ", parameters)}) {{\n{Body}\n}}";
    }
}

public record MonkeyBuiltin(BuiltinFunction Fn) : IObject {
    public ObjectType Type() => ObjectType.BUILTIN_FUNCTION;

    public string Inspect() => "builtin function";
}

public record MonkeyArray(List<IObject> Elements) : IObject {
    public ObjectType Type() => ObjectType.ARRAY;
    public string Inspect() {
        var elements = Elements.Select(e => e.Inspect());
        return $"[{string.Join(", ", elements)}]";
    }
}
