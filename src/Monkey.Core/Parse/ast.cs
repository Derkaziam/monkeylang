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
public record ProgramNode() : INode {
    public List<IStatement> Statements = new();
    public string TokenLiteral() => Statements.Count > 0 ? Statements[0].TokenLiteral() : "";

    public override string ToString() {
        foreach (var s in Statements) Console.WriteLine(s.ToString());
        return "";
    }
}

// ********** //
// Statements //
// ********** //
public record LetStatement(Token Tok, Identifier Name, IExpression Value) : IStatement {
    public void StatementNode() { }
    
    public string TokenLiteral() { return Tok.Value; }

    public override string ToString() => $"let {Name} = {Value}";
}

public record ReturnStatement(Token Tok, IExpression ReturnValue) : IStatement {
    public void StatementNode() { }
    
    public string TokenLiteral() { return Tok.Value; }

    public override string ToString() {
        return $"return {ReturnValue}";
    }
}

public record ExpressionStatement(Token Tok, IExpression Expression) : IStatement {
    public void StatementNode() { }
    public string TokenLiteral() { return Tok.Value; }
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

public record ArrayLiteral(Token Tok, List<IExpression> Elements) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() => Tok.Value;
    public override string ToString() {
        string str = "[";
        foreach (IExpression e in Elements) {
            str += " " + e.ToString() + ",";
        }
        return str += "]";
    }
}

public record BooleanLiteral : IExpression {
    public Token Tok;
    public bool Value;
    public BooleanLiteral(Token tok, bool value) { Tok = tok; Value = value; }
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return Value.ToString(); }
}

public record StringLiteral(Token Tok, string Value) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return Value.ToString(); }
}

public record PrefixExpression(Token Tok, string Operator, IExpression Right) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return $"({Operator}{Right})"; }
}

public record InfixExpression(Token Tok, IExpression Left, string Operator, IExpression Right) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { return $"({Left} {Operator} {Right})"; }
}

public record IfExpression(Token Tok, IExpression Condition, BlockStatement Consequence, BlockStatement Alternative) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() { 
        if (Alternative == null) {
            return $"if ({Condition}) {Consequence}";
        }
        return $"if ({Condition}) {{{Consequence} }} else {{{Alternative} }}";
    }
}

public record FunctionLiteral(Token Tok, List<Identifier> Parameters, BlockStatement Body) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() {
        if (Parameters == null) return $"fn() {{{Body} }}";

        List<string> str = new();
        foreach (var p in Parameters) str.Add(p.ToString());

        return $"fn({string.Join(", ", str)}) {{{Body} }}";
    }
}

public record CallExpression(Token Tok, IExpression Function, List<IExpression> Arguments) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() {
        if (Arguments == null) return $"{Function}()";
        
        List<string> str = new();
        foreach (var a in Arguments) str.Add(a.ToString());

        return $"{Function}({string.Join(", ", str)})";
    }
}

public record IndexExpression(Token Tok, IExpression Left, IExpression Index) : IExpression {
    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public override string ToString() {
        return $"({Left}[{Index}])";
    }
}