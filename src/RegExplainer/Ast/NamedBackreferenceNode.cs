namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class NamedBackreferenceNode : RegexNode
{
	public NamedBackreferenceNode(string name)
	{
		this.Name = name;
	}

	public string Name { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitNamedBackreference(this, indent);
}
