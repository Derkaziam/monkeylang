using Monk.Core.Lex;

namespace Monk.Core.Parse;

using PrefixParseFn = Func<IExpression?>;
using InfixParseFn = Func<IExpression, IExpression?>;


public class Parser {
    readonly Lexer lexer;
    Token curToken;
    Token peekToken;
    public List<string> errors;
    readonly Dictionary<TokenType, PrefixParseFn> prefixParseFns = new();
    readonly Dictionary<TokenType, InfixParseFn> infixParseFns = new();

    private enum PrecedenceLevel {
        LOWEST,
        EQUALS, // ==
        LESSGREATER, // > or <
        SUM, // +
        PRODUCT, // *
        PREFIX, // -X or !X
        CALL, // myFunction(X)
        INDEX, // array[index]
    }

    readonly Dictionary<TokenType, PrecedenceLevel> precedences = new() {
        { TokenType.EQ, PrecedenceLevel.EQUALS },
        { TokenType.NOT_EQ, PrecedenceLevel.EQUALS },
        { TokenType.LT, PrecedenceLevel.LESSGREATER },
        { TokenType.GT, PrecedenceLevel.LESSGREATER },
        { TokenType.PLUS, PrecedenceLevel.SUM },
        { TokenType.MINUS, PrecedenceLevel.SUM },
        { TokenType.SLASH, PrecedenceLevel.PRODUCT },
        { TokenType.ASTERISK, PrecedenceLevel.PRODUCT },
        { TokenType.LPAREN, PrecedenceLevel.CALL },
        { TokenType.LBRACKET, PrecedenceLevel.INDEX },
    };

    public Parser(Lexer l) {
        lexer = l;
        errors = new List<string>();
        curToken = lexer.NextToken();
        peekToken = lexer.NextToken();
        
        // Prefixes
        RegisterPrefix(TokenType.IDENT, ParseIdentifier);
        RegisterPrefix(TokenType.INT, ParseIntegerLiteral);
        RegisterPrefix(TokenType.BANG, ParsePrefixExpression);
        RegisterPrefix(TokenType.MINUS, ParsePrefixExpression);
        RegisterPrefix(TokenType.TRUE, ParseBoolean);
        RegisterPrefix(TokenType.FALSE, ParseBoolean);
        RegisterPrefix(TokenType.LPAREN, ParseGroupedExpression);
        RegisterPrefix(TokenType.IF, ParseIfExpression);
        RegisterPrefix(TokenType.FUNCTION, ParseFunctionLiteral);
        RegisterPrefix(TokenType.STRING, ParseStringLiteral);
        RegisterPrefix(TokenType.LBRACKET, ParseArrayLiteral);

        // Infixes
        RegisterInfix(TokenType.PLUS, ParseInfixExpression);
        RegisterInfix(TokenType.MINUS, ParseInfixExpression);
        RegisterInfix(TokenType.SLASH, ParseInfixExpression);
        RegisterInfix(TokenType.ASTERISK, ParseInfixExpression);
        RegisterInfix(TokenType.EQ, ParseInfixExpression);
        RegisterInfix(TokenType.NOT_EQ, ParseInfixExpression);
        RegisterInfix(TokenType.LT, ParseInfixExpression);
        RegisterInfix(TokenType.GT, ParseInfixExpression);
        RegisterInfix(TokenType.LPAREN, ParseCallExpression);
        RegisterInfix(TokenType.LBRACKET, ParseIndexExpression);

    }

    public List<string> Errors() {
        return errors;
    }
    
    public void PeekError(TokenType t) {
        errors.Add($"expected next token to be {t}, got {peekToken.Type} instead");
    }
    
    private void NoPrefixParseFnError(TokenType t) {
        errors.Add($"no prefix parse function for {t} found");
    }

    public ProgramNode ParseProgram() {
        ProgramNode p = new ();

        while (curToken.Type != TokenType.EOF) {
            IStatement? stmt = ParseStatement();
            if (stmt != null) {
                p.Statements.Add(stmt);
            }
            NextToken();
        }

        return p;
    }

    // ************** //
    // Helper Methods //
    // ************** //
    private void NextToken() {
        curToken = peekToken;
        peekToken = lexer.NextToken();
    }

    private bool CurTokenIs(TokenType t) {
        return curToken.Type == t;
    }

    private bool PeekTokenIs(TokenType t) {
        return peekToken.Type == t;
    }

    private void RegisterPrefix(TokenType t, PrefixParseFn fn) => prefixParseFns[t] = fn;

    private void RegisterInfix(TokenType t, InfixParseFn fn) => infixParseFns[t] = fn;

    private bool ExpectPeek(TokenType t) {
        if (PeekTokenIs(t)) {
            NextToken();
            return true;
        } else {
            PeekError(t);
            return false;
        }
    }

    private PrecedenceLevel PeekPrecedence() {
        bool ok = precedences.TryGetValue(peekToken.Type, out PrecedenceLevel value);
        if (!ok) return PrecedenceLevel.LOWEST;
        return value;
    }

    private PrecedenceLevel CurPrecedence() {
        bool ok = precedences.TryGetValue(curToken.Type, out PrecedenceLevel value);
        if (!ok) return PrecedenceLevel.LOWEST;
        return value;
    }

    // ************************* //
    // Statement Parsing Methods //
    // ************************* //
    private IStatement? ParseStatement() {
        return curToken.Type switch {
            TokenType.LET => ParseLetStatement(),
            TokenType.RETURN => ParseReturnStatement(),
            _ => ParseExpressionStatement(),
        };
    }
    private LetStatement? ParseLetStatement() {
        Token tok = curToken;

        if (!ExpectPeek(TokenType.IDENT)) return null;

        Identifier name = new(curToken, curToken.Value);
        if (!ExpectPeek(TokenType.ASSIGN)) return null;

        NextToken();
        IExpression? value = ParseExpression(PrecedenceLevel.LOWEST);
        
        if (value == null) return null;
        if (PeekTokenIs(TokenType.SEMICOLON)) NextToken();

        return new LetStatement(tok, name, value);
    }

    private ReturnStatement? ParseReturnStatement() {
        Token tok = curToken;

        NextToken();
        IExpression? value = ParseExpression(PrecedenceLevel.LOWEST);
        
        if (value == null) return null;
        if (PeekTokenIs(TokenType.SEMICOLON)) NextToken();

        return new ReturnStatement(tok, value);
    }
    
    private ExpressionStatement? ParseExpressionStatement() {
        IExpression? expression = ParseExpression(PrecedenceLevel.LOWEST);
        
        if (expression == null) return null;

        if (PeekTokenIs(TokenType.SEMICOLON)) NextToken();

        return new ExpressionStatement(curToken, expression);
    }

    private BlockStatement? ParseBlockStatement() {
        Token tok = curToken;
        var statements = new List<IStatement>();
        NextToken();

        while (!CurTokenIs(TokenType.RBRACE) && !CurTokenIs(TokenType.EOF)) {
            IStatement? stmt = ParseStatement();
            if (stmt != null) statements.Add(stmt);
            NextToken();
        }

        return new BlockStatement(tok, statements);
    }

    // ************************** //
    // Expression Parsing Methods //
    // ************************** //
    private IExpression? ParseExpression(PrecedenceLevel precedence) {
        bool ok = prefixParseFns.TryGetValue(curToken.Type, out PrefixParseFn? prefix);

        if (!ok) {
            NoPrefixParseFnError(curToken.Type);
            return null;
        }

        IExpression? leftExp = prefix!();
        if (leftExp == null) return null;

        while (!PeekTokenIs(TokenType.SEMICOLON) && precedence < PeekPrecedence()) {
            ok = infixParseFns.TryGetValue(peekToken.Type, out InfixParseFn? infix);
            if (!ok) return leftExp;
            NextToken();
            leftExp = infix!(leftExp);
            if (leftExp == null) return null;
        }

        return leftExp;
    }

    private IExpression? ParsePrefixExpression() {
        Token tok = curToken;

        NextToken();
        return new PrefixExpression(tok, tok.Value, ParseExpression(PrecedenceLevel.PREFIX));
    }

    private IExpression? ParseInfixExpression(IExpression left) {
        Token tok = curToken;

        PrecedenceLevel precedence = CurPrecedence();
        NextToken();
        IExpression? right = ParseExpression(precedence);

        return new InfixExpression(tok, left, tok.Value, right);
    }

    private IExpression? ParseIdentifier() => new Identifier(curToken, curToken.Value);
    private IExpression? ParseStringLiteral() => new StringLiteral(curToken, curToken.Value);

    private IExpression? ParseIntegerLiteral() {
        if (!long.TryParse(curToken.Value, out long val))
            errors.Add($"could not parse {curToken.Value} as integer");

        return new IntegerLiteral(curToken, val);
    }

    private IExpression? ParseBoolean() => new BooleanLiteral(curToken, curToken.Type == TokenType.TRUE);

    private IExpression? ParseGroupedExpression() {
        NextToken();
        IExpression? exp = ParseExpression(PrecedenceLevel.LOWEST);
        if (!ExpectPeek(TokenType.RPAREN)) return null;

        return exp;
    }

    private IExpression? ParseIfExpression() {
        Token tok = curToken;

        // Parse condition
        if (!ExpectPeek(TokenType.LPAREN)) return null;
        NextToken();
        IExpression? condition = ParseExpression(PrecedenceLevel.LOWEST);
        if (!ExpectPeek(TokenType.RPAREN)) return null;
        
        
        if (!ExpectPeek(TokenType.LBRACE)) return null;
        BlockStatement? consequence = ParseBlockStatement();

        if (PeekTokenIs(TokenType.ELSE)) {
            NextToken();
            if (!ExpectPeek(TokenType.LBRACE)) return null;
            BlockStatement? alternative = ParseBlockStatement();
            return new IfExpression(tok, condition, consequence, alternative);
        }

        return new IfExpression(tok, condition, consequence, null);
    }

    private IExpression? ParseFunctionLiteral() {
        Token tok = curToken;

        if (!ExpectPeek(TokenType.LPAREN)) return null;

        List<Identifier>? Parameters = ParseFunctionParameters(); 

        if (!ExpectPeek(TokenType.LBRACE)) return null;

        BlockStatement? body = ParseBlockStatement();

        return new FunctionLiteral(tok, Parameters, body);
    }

    private List<Identifier>? ParseFunctionParameters() {
        List<Identifier> identifiers = new();

        if (PeekTokenIs(TokenType.RPAREN)) {
            NextToken();
            return identifiers;
        }

        NextToken();

        Identifier ident = new(curToken, curToken.Value);
        identifiers.Add(ident);

        while (PeekTokenIs(TokenType.COMMA)) {
            NextToken();
            NextToken();

            ident = new(curToken, curToken.Value);
            identifiers.Add(ident);
        }

        if (!ExpectPeek(TokenType.RPAREN)) return null;

        return identifiers;
    }

    private IExpression? ParseArrayLiteral() {
        Token tok = curToken;
        List<IExpression>? elements = ParseExpressionList(TokenType.RBRACKET);
        if (elements == null) return null;
        return new ArrayLiteral(tok, elements);
    }

    private List<IExpression>? ParseExpressionList(TokenType end) {
        List<IExpression> list = new();

        if (PeekTokenIs(end)) {
            NextToken();
            return list;
        }

        NextToken();
        list.Add(ParseExpression(PrecedenceLevel.LOWEST));
        
        while (PeekTokenIs(TokenType.COMMA)) {
            NextToken();
            NextToken();
            list.Add(ParseExpression(PrecedenceLevel.LOWEST));
        }

        if (!ExpectPeek(end)) return null;
        return list;
    }

    private IExpression? ParseCallExpression(IExpression function) {
        return new CallExpression(curToken, function, ParseExpressionList(TokenType.RPAREN));
    }
    
    private IExpression? ParseIndexExpression(IExpression left) {
        Token tok = curToken;
        NextToken();
        IExpression? index = ParseExpression(PrecedenceLevel.LOWEST);
        if (!ExpectPeek(TokenType.RBRACKET)) return null;
        return new IndexExpression(tok, left, index);
    }

    private List<IExpression>? ParseCallArguments() {
        List<IExpression> args = new();

        if (PeekTokenIs(TokenType.RPAREN)) {
            NextToken();
            return args;
        }

        NextToken();
        args.Add(ParseExpression(PrecedenceLevel.LOWEST));

        while (PeekTokenIs(TokenType.COMMA)) {
            NextToken();
            NextToken();
            args.Add(ParseExpression(PrecedenceLevel.LOWEST));
        }

        if (!ExpectPeek(TokenType.RPAREN)) return null;

        return args;
    }
}

