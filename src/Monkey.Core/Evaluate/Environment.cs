namespace Monk.Core.Evaluate;

public record MonkeyEnvironment() {
    private Dictionary<string, IObject> Store = new();
    private MonkeyEnvironment? Outer { get; set; } = null;

    public static MonkeyEnvironment NewEnclosedEnvironment(MonkeyEnvironment outer) {
        return new() { Outer = outer };
    }
    
    public (IObject?, bool) Get(string name) {
        var ok = Store.TryGetValue(name, out IObject? value);

        if (!ok && Outer != null)
            return Outer.Get(name);
        return (value, ok);
    }

    public IObject Set(string name, IObject value) {
        Store[name] = value;
        return value;
    }
}