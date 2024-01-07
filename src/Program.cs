using Monk.Core.Lex;
using Monk.Core.Tok;
using Monk.Core.Repl;
using Monk.Core.Parse;

namespace Monk.Core;

class Program {
    static void Main(string[] args) {
        if (args.Length > 0 && args[0] == "repl") REPL();

        string input = @"let x = 5;
let y = 10;
let z = 838383;
return 0;
!true;
-0;
9 + 9 - 9;
a + b * (c + d) / e - f;
if (x < y) { x; } else { y; }
fn(x) { return x; };
fn(x, y, z) {};
add(2 + 2);";
        Lexer l = new(input);
        Parser p = new(l);

        var program = p.ParseProgram();
        
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
