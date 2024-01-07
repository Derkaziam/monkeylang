namespace Monk.Core.Lex;

/// <summary>
/// Token is a struct that represents a token.
/// <param name="Value"></param>
/// <param name="Type"></param>
/// </summary>
public struct Token {
    public string Value;
    public TokenType Type;

    public override string ToString() {
        return $"{Type}: {Value}";
    }
}

public static class TokenKey {
    static Dictionary<string, TokenType> keywords = new() {
        { "fn", TokenType.FUNCTION },
        { "let", TokenType.LET },
        { "true", TokenType.TRUE },
        { "false", TokenType.FALSE },
        { "if", TokenType.IF },
        { "else", TokenType.ELSE },
        { "return", TokenType.RETURN },
    };

    /// <summary>
    /// LookupIdent is a public static function that takes a string parameter called ident.
    /// </summary>
    /// <param name="ident"></param>
    /// <returns>- TokenType</returns>
    public static TokenType LookupIdent(string ident) {
        return keywords.GetValueOrDefault(ident, TokenType.IDENT);
    }
} 

/// <summary>
/// TokenType is an enum that represents the type of token.
/// </summary>
public enum TokenType {
    ILLEGAL,
    EOF,

    IDENT,
    INT,
    TRUE,
    FALSE,

    // Operators
    ASSIGN,
    PLUS,
    MINUS,
    BANG,
    ASTERISK,
    SLASH,

    LT,
    GT,
    EQ,
    NOT_EQ,

    // Seperators
    COMMA,
    SEMICOLON,

    LPAREN,
    RPAREN,
    LBRACE,
    RBRACE,

    FUNCTION,
    LET,
    IF,
    ELSE,
    RETURN,
}
