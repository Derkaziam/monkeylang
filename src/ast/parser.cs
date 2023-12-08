using Monk.Core.Tok;
using Monk.Core.Lex;

namespace Monk.Core.Parse;

public delegate Expression PrefixParseFn();
public delegate Expression InfixParseFn(Expression expr);

public class Parser {
    Lexer lexer;
    Token curToken;
    Token peekToken;
    public List<string> errors;
    Dictionary<TokenType, PrefixParseFn> prefixParseFns = new ();
    Dictionary<TokenType, InfixParseFn> infixParseFns = new ();

    public Parser(Lexer l) {
        lexer = l;
        errors = new List<string>();
        curToken = lexer.NextToken();
        peekToken = lexer.NextToken();
    }

    public List<string> Errors() {
        return errors;
    }

    public void PeekError(TokenType t) {
        var msg = $"expected next token to be {t}, got {peekToken.Type} instead";
        errors.Add(msg);
    }

    public ProgramNode ParseProgram() {
        ProgramNode p = new ();

        while (curToken.Type != TokenType.EOF) {
            Statement stmt = ParseStatement();
            if (stmt != null) {
                p.Statements.Add(stmt);
            }
            NextToken();
        }

        return p;
    }

    private void NextToken() {
        curToken = peekToken;
        peekToken = lexer.NextToken();
    }

    private Statement ParseStatement() {
        switch (curToken.Type) {
            case TokenType.LET:
                return ParseLetStatement();
            case TokenType.RETURN:
                return ParseReturnStatement();
            default:
                return ParseExpressionStatement();
        }
    }

    private LetStatement ParseLetStatement() {
        LetStatement stmt = new (curToken);

        if (!ExpectPeek(TokenType.IDENT)) {
            return null;
        }

        stmt.Name = new Identifier(curToken, curToken.Value);

        if (!ExpectPeek(TokenType.ASSIGN)) {
            return null;
        }

        while (!CurTokenIs(TokenType.SEMICOLON)) {
            NextToken();
        }

        return stmt;
    }

    private ReturnStatement ParseReturnStatement() {
        ReturnStatement stmt = new (curToken);

        NextToken();

        while (!CurTokenIs(TokenType.SEMICOLON)) {
            NextToken();
        }

        return stmt;
    }
    
    private ExpressionStatement ParseExpressionStatement() {
        ExpressionStatement stmt = new (curToken);

        NextToken();

        while (!PeekTokenIs(TokenType.SEMICOLON)) {
            NextToken();
        }

        return stmt;
    }

    private bool CurTokenIs(TokenType t) {
        return curToken.Type == t;
    }

    private bool PeekTokenIs(TokenType t) {
        return peekToken.Type == t;
    }

    private bool ExpectPeek(TokenType t) {
        if (PeekTokenIs(t)) {
            NextToken();
            return true;
        } else {
            PeekError(t);
            return false;
        }
    }

    private void RegisterPrefix(TokenType t, PrefixParseFn fn) {
        prefixParseFns[t] = fn;
    }

    private void RegisterInfix(TokenType t, InfixParseFn fn) {
        infixParseFns[t] = fn;
    }
}

