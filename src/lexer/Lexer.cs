using Monk.Tok;

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
        case '=':
            tok = NewToken(TokenType.ASSIGN, "=");
        case ';':
            tok = NewToken(TokenType.SEMICOLON)
        case '(':
            tok = NewToken(TokenType.LPAREN)
        case ')':
            tok = NewToken(TokenType.RPAREN)
        case '{':
            tok = NewToken(TokenType.LBRACE)
        case '}':
            tok = NewToken(TokenType.RBRACE)
        case ',':
            tok = NewToken(TokenType.COMMA)
        case '+':
            tok = NewToken(TokenType.PLUS)
        case '-':
            tok = NewToken(TokenType.MINUS)
        case '*':
            tok = NewToken(TokenType.ASTERISK)
        case '/':
            tok = NewToken(TokenType.SLASH)
        case '!':
            tok = NewToken(TokenType.BANG)
        }
    }

    private Token NewToken(TokenType type, string lit = currentChar.ToString()) {
        return (Token) { Type = type, Value = lit };
    }
}
