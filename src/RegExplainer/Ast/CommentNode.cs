namespace RegExplainer.Ast;

using RegExplainer.Visitor;

public sealed class CommentNode : RegexNode
{
	public CommentNode(string text) => this.Text = text;

	public string Text { get; }

	public override void Accept(IRegexVisitor visitor, int indent) => visitor.VisitComment(this, indent);
}
