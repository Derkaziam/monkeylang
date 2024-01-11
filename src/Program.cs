using Monk.Core.Lex;
using Monk.Core.Repl;
using Monk.Core.Parse;
using Monk.Core.Evaluate;

namespace Monk.Core;

class Program {
    static void Main(string[] args) {
        MonkeyEnvironment env = new();
        string input = @"true == true;";

        if (args.Length > 0 && args[0] == "repl") REPL();
        else if (args.Length > 0) input = File.ReadAllText(args[0]); 

        Lexer l = new(input);
        Parser p = new(l);

        var program = p.ParseProgram();

        
        if (program == null) {
            Console.WriteLine("No program");
            return;
        }

        if (p.errors.Count > 0) {
            foreach (string s in p.errors) {
                Console.WriteLine(s);
            }
        }

        var e = Evaluator.Eval(program, env);
    }

    public static void REPL() {
        string user = Environment.UserName;
        Console.WriteLine($"Hello {user}! This is the Monkey programming language!");
        Console.WriteLine("Feel free to type in commands");

        REPL repl = new();
        repl.Start(Console.In, Console.Out);
    }
}
