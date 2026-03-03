namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class LiteralNode : RegexNode
{
	public LiteralNode(string t) => this.Text = t;

	public string Text { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitLiteral(this, indent);
}
