namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class BackreferenceNode : RegexNode
{
	public BackreferenceNode(int num)
	{
		this.Number = num;
	}

	public int Number { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitBackreference(this, indent);
}
