using System.Collections.Generic;
using Monk.Core.Parse;

namespace Monk.Core.Evaluate;

public class MonkeyEnvironment {
    private Dictionary<string, IObject> Store { get; init; }
    private MonkeyEnvironment? Outer { get; set; }

    public MonkeyEnvironment() =>
        Store = new Dictionary<string, IObject>();

    private static MonkeyEnvironment NewEnvironment() =>
        new() { Store = new Dictionary<string, IObject>(), Outer = null };

    public static MonkeyEnvironment NewEnclosedEnvironment(MonkeyEnvironment outer) {
        var env = NewEnvironment();
        env.Outer = outer;
        return env;
    }

    // TODO: Why return a tuple and not simply null if not found? Can IObject ever be null?
    public (IObject?, bool) Get(string name) {
        var ok = Store.TryGetValue(name, out var value);

        // If current environment doesn't have a value associated with a
        // name, we recursively call Get on enclosing environment (which the
        // current environment is extending) until either name is found or
        // caller can issue a "ERROR: Unknown identifier: foobar" error.
        if (!ok && Outer != null)
            return Outer.Get(name);
        return (value, ok);
    }

    public IObject Set(string name, IObject val) {
        Store[name] = val;
        return val;
    }
}