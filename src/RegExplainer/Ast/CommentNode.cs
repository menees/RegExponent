namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class CommentNode : RegexNode
{
	public CommentNode(string text, bool isInline = false)
	{
		this.Text = text;
		this.IsInline = isInline;
	}

	public bool IsInline { get; }

	public string Text { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitComment(this, indent);
}
