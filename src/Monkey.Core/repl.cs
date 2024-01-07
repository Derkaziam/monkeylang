using System.ComponentModel;
using Monk.Core.Evaluate;
using Monk.Core.Lex;
using Monk.Core.Parse;

namespace Monk.Core.Repl;

public class REPL {
    const string prompt = "Monkey >> ";

    public void Start(TextReader input, TextWriter output) {
        while (true) {
            output.Write(prompt);
            var line = input.ReadLine();
            if (line == null) {
                break;
            }
            
            Lexer l = new(line);
            Parser p = new(l);

            var program = p.ParseProgram();
            if (p.Errors().Count != 0) {
                PrintParseErrors(output, p.Errors());
                continue;
            }

            var evaluated = Evaluator.Eval(program);
            if (evaluated != null) {
                Console.WriteLine(evaluated.Inspect());
            }
        }
    }

    private void PrintParseErrors(TextWriter output, List<string> errors) {
        foreach (string s in errors) {
            Console.WriteLine(s);
        }
    }
}