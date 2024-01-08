using Monk.Core.Lex;

namespace Monk.Core.Parse;

// ***** //
// Nodes //
// ***** //
public interface INode { 
    public string TokenLiteral();
} 
public interface IStatement : INode {
    public void StatementNode();
    public string ToString();
} 
public interface IExpression : INode {
    public void ExpressionNode();
    public string ToString();
} 
public record ProgramNode : INode {
    public List<IStatement> Statements;

    public ProgramNode() { Statements = new List<IStatement>(); }
    
    public string TokenLiteral() {
        if (Statements.Count > 0) {
            return Statements[0].TokenLiteral();
        } else {
            return "";
        }
    }

    public override string ToString() {
        foreach (var s in Statements) {
            Console.WriteLine(s.ToString());
        }
        return "";
    }
}

// ********** //
// Statements //
// ********** //
public record LetStatement : IStatement {
    public Token Tok;
    public Identifier? Name;
    public IExpression? Value;

    public void StatementNode() { }
    
    public string TokenLiteral() { return Tok.Value; }

    public LetStatement(Token tok, Identifier? name, IExpression? value) {
        Tok = tok;
        Name = name;
        Value = value;
    }

    public override string ToString() {
        return $"let {Name} = {Value}";
    }
}

public record ReturnStatement : IStatement {
    public Token Tok;
    public IExpression? ReturnValue;

    public void StatementNode() { }
    
    public string TokenLiteral() { return Tok.Value; }

    public ReturnStatement(Token tok, IExpression? returnValue) {
        Tok = tok;
        ReturnValue = returnValue;
    }

    public override string ToString() {
        return $"return {ReturnValue}";
    }
}

public record ExpressionStatement : IStatement {
    public Token Tok;
    public IExpression? Expression;

    public void StatementNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public ExpressionStatement(Token tok, IExpression? exp) { Tok = tok; Expression = exp; }

    public override string ToString() {
        return $"{Expression}";
    }
}

public record BlockStatement : IStatement {
    public Token Tok;
    public List<IStatement> Statements;
    public BlockStatement(Token tok, List<IStatement> statements) {
        Tok = tok;
        Statements = statements;
    }
    public void StatementNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() {
        string str = "";
        foreach (var s in Statements) {
            str += " " + s.ToString() + ";";
        }
        return str;
    }
}

// *********** //
// Expressions //
// *********** //
public record Identifier : IExpression {
    public Token Tok;
    public string Value;

    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public Identifier(Token tok, string value) { Tok = tok; Value = value; }

    public override string ToString() {
        return Value;
    }
}

public record IntegerLiteral : IExpression {
    public Token Tok;
    public long Value;
    public IntegerLiteral(Token tok, long value) { Tok = tok; Value = value; }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return Value.ToString(); }
}

public record PrefixExpression : IExpression {
    public Token Tok;
    public string Operator;
    public IExpression? Right;
    public PrefixExpression(Token tok, string op, IExpression? right) {
        Tok = tok;
        Operator = op;
        Right = right;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return $"({Operator}{Right})"; }
}

public record InfixExpression : IExpression {
    public Token Tok;
    public IExpression? Left;
    public string Operator;
    public IExpression? Right;
    public InfixExpression(Token tok, IExpression? left, string op, IExpression? right) {
        Tok = tok;
        Left = left;
        Operator = op;
        Right = right;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return $"({Left} {Operator} {Right})"; }
}

public record BooleanLiteral : IExpression {
    public Token Tok;
    public bool Value;
    public BooleanLiteral(Token tok, bool value) { Tok = tok; Value = value; }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return Value.ToString(); }
}

public record IfExpression : IExpression {
    public Token Tok;
    public IExpression? Condition;
    public BlockStatement? Consequence;
    public BlockStatement? Alternative;
    public IfExpression(Token tok, IExpression? condition, BlockStatement? consequence, BlockStatement? alternative) {
        Tok = tok;
        Condition = condition;
        Consequence = consequence;
        Alternative = alternative;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { 
        if (Alternative == null) {
            return $"if ({Condition}) {Consequence}";
        }
        return $"if ({Condition}) {{{Consequence} }} else {{{Alternative} }}";
    }
}

public record FunctionLiteral : IExpression {
    public Token Tok;
    public List<Identifier>? Parameters;
    public BlockStatement? Body;
    public FunctionLiteral(Token tok, List<Identifier>? parameters, BlockStatement? body) {
        Tok = tok;
        Parameters = parameters;
        Body = body;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() {
        if (Parameters == null) return $"fn() {{{Body} }}";

        List<string> str = new();
        foreach (var p in Parameters) str.Add(p.ToString());

        return $"fn({string.Join(", ", str)}) {{{Body} }}";
    }
}

public record CallExpression : IExpression {
    public Token Tok;
    public IExpression? Function;
    public List<IExpression>? Arguments;
    public CallExpression(Token token, IExpression? function, List<IExpression>? arguments) {
        Tok = token;
        Function = function;
        Arguments = arguments;
    }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() {
        if (Arguments == null) return $"{Function}()";
        
        List<string> str = new();
        foreach (var a in Arguments) str.Add(a.ToString());

        return $"{Function}({string.Join(", ", str)})";
    }
}
