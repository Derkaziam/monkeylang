using Monk.Core.Lex;
using Monk.Core.Tok;
using Monk.Core.Repl;
using Monk.Core.Parse;

namespace Monk.Core;

class Program {
    static void Main(string[] args) {
        string input = @"let x = 5;
let y = 10;
let z = 838383;
return x + y;";
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

    public void REPL() {
        string user = Environment.UserName;
        Console.WriteLine($"Hello {user}! This is the Monkey programming language!");
        Console.WriteLine("Feel free to type in commands");

        REPL repl = new();
        repl.Start(Console.In, Console.Out);
    }
}
