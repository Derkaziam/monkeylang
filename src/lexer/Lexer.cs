using Monk.Core.Tok;

namespace Monk.Core.Lex;

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

    public List<Token> Lex() {
        var tokens = new List<Token>();
        while (currentChar != '\0') {
            var tok = NextToken();
            tokens.Add(tok);
        }
        return tokens;
    }

    private void ReadChar() {
        currentChar = readPosition >= input.Length 
                    ? '\0' : input[readPosition];
        position = readPosition;
        readPosition++;
    }

    public Token NextToken() {
        Token tok;
        SkipWhitespace();
        switch (currentChar) {
        case '=':
            if (PeekChar() == '=') {
                ReadChar();
                tok = NewToken(TokenType.EQ, "==");
            } else {
                tok = NewToken(TokenType.ASSIGN, "=");
            }
            break;
        case ';':
            tok = NewToken(TokenType.SEMICOLON, ";");
            break;
        case '(':
            tok = NewToken(TokenType.LPAREN, "(");
            break;
        case ')':
            tok = NewToken(TokenType.RPAREN, ")");
            break;
        case '{':
            tok = NewToken(TokenType.LBRACE, "{");
            break;
        case '}':
            tok = NewToken(TokenType.RBRACE, "}");
            break;
        case ',':
            tok = NewToken(TokenType.COMMA, ",");
            break;
        case '+':
            tok = NewToken(TokenType.PLUS, "+");
            break;
        case '-':
            tok = NewToken(TokenType.MINUS, "-");
            break;
        case '!':
            if (PeekChar() == '=') {
                ReadChar();
                tok = NewToken(TokenType.NOT_EQ, "!=");
            } else {
                tok = NewToken(TokenType.BANG, "!");
            }
            break;
        case '/':
            tok = NewToken(TokenType.SLASH, "/");
            break;
        case '*':
            tok = NewToken(TokenType.ASTERISK, "*");
            break;
        case '<':
            tok = NewToken(TokenType.LT, "<");
            break;
        case '>':
            tok = NewToken(TokenType.GT, ">");
            break;
        case '\0':
            tok = NewToken(TokenType.EOF, "\0");
            break;
        default:
            if (IsLetter(currentChar)) {
                string ident = ReadIdentifier();
                tok = NewToken(TokenKey.LookupIdent(ident), ident);
                return tok;
            } else if (IsDigit(currentChar)) {
                tok = NewToken(TokenType.INT, ReadNumber());
                return tok;
            } else {
                tok = NewToken(TokenType.ILLEGAL, currentChar.ToString());
            }
            break;
        }

        ReadChar();
        return tok;
    }

    private Token NewToken(TokenType type, string lit) {
        return new Token { Type = type, Value = lit };
    }

    private bool IsLetter(int ch) {
        return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || ch == '_';
    }

    private bool IsDigit(int ch) {
        return ch >= '0' && ch <= '9';
    }

    private string ReadIdentifier() {
        int pos = position;
        while (IsLetter(currentChar)) {
            ReadChar();
        }
        return input[pos..position];
    }

    private string ReadNumber() {
        int pos = position;
        while (IsDigit(currentChar)) {
            ReadChar();
        }
        return input[pos..position];
    }

    private void SkipWhitespace() {
        while (currentChar == ' '  || currentChar == '\t'
            || currentChar == '\n' || currentChar == '\r') {
            ReadChar();
        }
    }

    private char PeekChar() {
        return readPosition >= input.Length 
             ? '\0' : input[readPosition];
    }
}
