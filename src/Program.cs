using Monk.Core.Lex;
using Monk.Core.Repl;
using Monk.Core.Parse;
using Monk.Core.Evaluate;

namespace Monk.Core;

class Program {
    static void Main(string[] args) {
        if (args.Length > 0 && args[0] == "repl") REPL();

        string input = @"5;";
        Lexer l = new(input);
        Parser p = new(l);

        var program = p.ParseProgram();

        var e = Evaluator.Eval(program);

        Console.WriteLine(e.ToString());
        
        if (program == null) {
            Console.WriteLine("No program");
            return;
        }

        Console.WriteLine(program.ToString());
        foreach (string s in p.errors) {
            Console.WriteLine(s);
        }
    }

    public static void REPL() {
        string user = Environment.UserName;
        Console.WriteLine($"Hello {user}! This is the Monkey programming language!");
        Console.WriteLine("Feel free to type in commands");

        REPL repl = new();
        repl.Start(Console.In, Console.Out);
    }
}
