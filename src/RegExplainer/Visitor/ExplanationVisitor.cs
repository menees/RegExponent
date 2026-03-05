namespace RegExplainer.Visitor;

#region Using Directives

using System.Text;

#endregion

public sealed class ExplanationVisitor : ExplanationVisitorBase
{
	#region Public Properties

	public StringBuilder Builder { get; } = new();

	#endregion

	#region Protected Methods

	protected override void AppendNode(int indent, string text, ExplainNodeKind nodeKind, Ast.RegexNode node)
	{
		this.Builder.AppendLine($"{new string(' ', indent * 2)}- {text} {FormatSpan(node)}");
	}

	#endregion
}
