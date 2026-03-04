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

	protected override void AppendLine(int indent, string text, ExplainNodeKind nodeKind, Ast.RegexNode? node)
	{
		string span = node != null ? $" {FormatSpan(node)}" : string.Empty;
		this.Builder.AppendLine($"{new string(' ', indent * 2)}- {text}{span}");
	}

	#endregion
}
