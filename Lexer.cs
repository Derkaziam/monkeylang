using Monk.Token;

namespace Monk.Lex;

public class Lexer {
    public string input;
    public int position;
    public int readPosition;
    public int currentChar;

    public Lexer(string inp) {
        input = inp;
        position = 0;
        readPosition = 0;
        currentChar = 0;

        ReadChar();
    }

    private ReadChar() {
        if (readPosition >= input.Length) {
            currentChar = 0;
        } else {
            currentChar = input[readPosition];
        }
        position = readPosition;
        readPosition++;
    }

    private Token NextToken() {
        Token tok;
        switch (currentChar) {
        case '='
            tok = (Token){  }
        }
    }
}
