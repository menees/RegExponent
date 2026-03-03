namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class AnchorNode : RegexNode
{
	public AnchorNode(AnchorKind k) => this.Kind = k;

	public AnchorKind Kind { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitAnchor(this, indent);
}
