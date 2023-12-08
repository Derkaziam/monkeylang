using System;
using System.IO;
using Monk.Lex;
using Monk.Tok;

namespace Monk.Repl;

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

            List<Token> toks = l.Lex();
            foreach (Token t in toks) {
                output.WriteLine(t.ToString());
            }
        }
    }
}