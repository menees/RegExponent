namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class DotNode : RegexNode
{
	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitDot(this, indent);
}
