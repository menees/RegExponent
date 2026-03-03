namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class SequenceNode : RegexNode
{
	public List<RegexNode> Items { get; } = [];

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitSequence(this, indent);
}
