namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class LookaroundNode : RegexNode
{
	public LookaroundNode(RegexNode inner, LookaroundKind k)
	{
		this.Inner = inner;
		this.Kind = k;
	}

	public RegexNode Inner { get; }

	public LookaroundKind Kind { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitLookaround(this, indent);
}
