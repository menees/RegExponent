namespace RegExplainer.Ast;

public sealed class AlternationNode : RegexNode
{
	public List<RegexNode> Alternatives { get; } = [];

	public override void Accept(Visitor.IRegexVisitor visitor, int indent) => visitor.VisitAlternation(this, indent);
}
