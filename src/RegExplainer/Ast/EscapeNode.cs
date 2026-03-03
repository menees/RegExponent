namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class EscapeNode : RegexNode
{
	public EscapeNode(string t) => this.EscapeText = t;

	public string EscapeText { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitEscape(this, indent);
}
