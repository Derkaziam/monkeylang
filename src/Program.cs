using Monk.Lex;
using Monk.Tok;
using Monk.Repl;

namespace Monk.Core;

class Program {
    static void Main(string[] args) {
/*         string input = @"let five = 5;
let ten = 10;
let add = fn(x, y) {
    x + y;
};

let result = add(five, ten);
!-/*5;
5 < 10 > 5;

if (5 < 10) {
    return true;
} else {
    return false;
";
        Lexer l = new(input);
        List<Token> toks = l.Lex();
        foreach (Token t in toks) {
            Console.WriteLine(t.ToString());
        } */
        var user = Environment.UserName;
        Console.WriteLine($"Hello {user}! This is the Monkey programming language!");
        Console.WriteLine("Feel free to type in commands");

        REPL repl = new();
        repl.Start(Console.In, Console.Out);
    }
}
