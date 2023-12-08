using Monk.Core.Tok;

namespace Monk.Core.Parse;

public interface Node { 
    public string TokenLiteral();
} 
public interface Statement : Node {
    public void StatementNode();
} 
public interface Expression : Node {
    public void ExpressionNode();
} 

public class ProgramNode : Node {
    public List<Statement> Statements;

    public ProgramNode() { Statements = new List<Statement>(); }
    
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

public class LetStatement : Statement {
    public Token Tok;
    public Identifier? Name;
    public Expression? Value;

    public void StatementNode() { }
    
    public string TokenLiteral() { return Tok.Value; }

    public LetStatement(Token tok) { Tok = tok; }

    public override string ToString() {
        return $"let {Name} = {Value}";
    }
}

public class ReturnStatement : Statement {
    public Token Tok;
    public Expression? ReturnValue;

    public void StatementNode() { }
    
    public string TokenLiteral() { return Tok.Value; }

    public ReturnStatement(Token tok) { Tok = tok; }

    public override string ToString() {
        return $"return {ReturnValue}";
    }
}

public struct ExpressionStatement : Statement {
    public Token Tok;
    public Expression? Expression;

    public void StatementNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public ExpressionStatement(Token tok) { Tok = tok; }

    public override string ToString() {
        return $"{Expression}";
    }
}

public struct Identifier : Expression {
    public Token Tok;
    public string Value;

    public void ExpressionNode() { }
    public string TokenLiteral() { return Tok.Value; }
    public Identifier(Token tok, string value) { Tok = tok; Value = value; }

    public override string ToString() {
        return Value;
    }
}