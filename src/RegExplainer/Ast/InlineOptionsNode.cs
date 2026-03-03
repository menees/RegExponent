namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class InlineOptionsNode : RegexNode
{
	public InlineOptionsNode(string options)
	{
		this.Options = options;
	}

	public string Options { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitInlineOptions(this, indent);
}
