namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class GroupNode : RegexNode
{
	public GroupNode(RegexNode inner, bool isCapturing = true, string? name = null, string? inlineOptions = null)
	{
		this.Inner = inner;
		this.IsCapturing = isCapturing;
		this.Name = name;
		this.InlineOptions = inlineOptions;
	}

	public string? InlineOptions { get; }

	public RegexNode Inner { get; }

	public bool IsCapturing { get; }

	public string? Name { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitGroup(this, indent);
}
