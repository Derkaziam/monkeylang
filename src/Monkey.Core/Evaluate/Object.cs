namespace Monk.Core.Evaluate;

public enum ObjectType {
    NONE,
    ERROR,
    INTEGER,
    BOOLEAN,
    NULL,
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

public record MonkeyNull : IObject {
    public ObjectType Type() => ObjectType.NULL;
    public string Inspect() => $"null"; 
}
