namespace RegExplainer.Visitor;

#region Using Directives

using System.Text;
using RegExplainer.Ast;

#endregion

public sealed class ExplanationVisitor : IRegexVisitor
{
	#region Public Properties

	public StringBuilder Builder { get; } = new();

	#endregion

	#region Public Methods

	public void VisitAlternation(AlternationNode node, int indent)
	{
		this.AppendLine(indent, $"- Alternation (one of {node.Alternatives.Count} branches) {FormatSpan(node)}");
		for (int i = 0; i < node.Alternatives.Count; i++)
		{
			this.AppendLine(indent, $"  [{i}] Branch:");
			node.Alternatives[i].Accept(this, indent + 2);
		}
	}

	public void VisitAnchor(AnchorNode node, int indent)
	{
		string desc = node.Kind == Ast.AnchorKind.Start ? "start of input '^'" : "end of input '$'";
		this.AppendLine(indent, $"- Anchor: {desc} {FormatSpan(node)}");
	}

	public void VisitBackreference(BackreferenceNode node, int indent)
	{
		this.AppendLine(indent, $"- Backreference: \\{node.Number} {FormatSpan(node)}");
	}

	public void VisitComment(CommentNode node, int indent)
	{
		this.AppendLine(indent, $"- Comment: # {node.Text} {FormatSpan(node)}");
	}

	public void VisitCharacterClass(CharacterClassNode node, int indent)
	{
		this.AppendLine(indent, $"- Character class: [{(node.IsNegated ? "^" : string.Empty)}...] ({node.Items.Count} item(s)) {FormatSpan(node)}");
		string[] parts = [.. node.Items.Select(FormatClassItem)];
		this.AppendLine(indent, $"  Contents: {string.Join(", ", parts)}");
	}

	public void VisitConditional(ConditionalNode node, int indent)
	{
		this.AppendLine(indent, $"- Conditional: if ({node.Condition})");
		this.AppendLine(indent, "  True branch:");
		node.TrueBranch.Accept(this, indent + 2);
		if (node.FalseBranch != null)
		{
			this.AppendLine(indent, "  False branch:");
			node.FalseBranch.Accept(this, indent + 2);
		}
	}

	public void VisitDot(DotNode node, int indent)
	{
		this.AppendLine(indent, $"- Dot: matches any character (except newline by default) {FormatSpan(node)}");
	}

	public void VisitEscape(EscapeNode node, int indent)
	{
		string desc = DescribeEscape(node.EscapeText);
		this.AppendLine(indent, $"- Escape: {node.EscapeText} ({desc})");
	}

	public void VisitGroup(GroupNode node, int indent)
	{
		string kind = (node.IsCapturing, node.Name) switch
		{
			(true, string name) => $"capturing (named '{name}')",
			(true, _) => "capturing",
			_ => "non-capturing",
		};
		if (!string.IsNullOrEmpty(node.InlineOptions))
		{
			kind += $" (options: {node.InlineOptions})";
		}

		this.AppendLine(indent, $"- Group: {kind} {FormatSpan(node)}");
		node.Inner.Accept(this, indent + 1);
	}

	public void VisitInlineOptions(InlineOptionsNode node, int indent)
	{
		this.AppendLine(indent, $"- Inline options: (?{node.Options}) {FormatSpan(node)}");
	}

	public void VisitLiteral(LiteralNode node, int indent)
	{
		string disp = node.Text.Length == 1 ? QuoteChar(node.Text[0]) : $"\"{EscapeStringForDisplay(node.Text)}\"";
		this.AppendLine(indent, $"- Literal: {disp} {FormatSpan(node)}");
	}

	public void VisitLookaround(LookaroundNode node, int indent)
	{
		string desc = node.Kind switch
		{
			LookaroundKind.PositiveLookahead => "Positive lookahead (?=)",
			LookaroundKind.NegativeLookahead => "Negative lookahead (?!)",
			LookaroundKind.PositiveLookbehind => "Positive lookbehind (?<=)",
			LookaroundKind.NegativeLookbehind => "Negative lookbehind (?<!)",
			_ => "Lookaround",
		};
		this.AppendLine(indent, $"- {desc} {FormatSpan(node)}");
		node.Inner.Accept(this, indent + 1);
	}

	public void VisitNamedBackreference(NamedBackreferenceNode node, int indent)
	{
		this.AppendLine(indent, $"- Named backreference: \\k<{node.Name}> {FormatSpan(node)}");
	}

	public void VisitQuantifier(QuantifierNode node, int indent)
	{
		string range = (node.Min, node.Max) switch
		{
			(0, null) => "0 or more",
			(int min, null) => $"{min} or more",
			(int min, int max) when min == max => $"exactly {min}",
			(int min, int max) => $"{min}..{max}",
		};
		string mode = node.IsLazy ? "lazy" : "greedy";
		node.Target.Accept(this, indent);
		this.AppendLine(indent + 1, $"(repeated {range}, {mode}) {FormatSpan(node)}");
	}

	public void VisitSequence(SequenceNode node, int indent)
	{
		this.AppendLine(indent, $"- Sequence ({node.Items.Count} item(s)) {FormatSpan(node)}");
		foreach (RegexNode child in node.Items)
		{
			child.Accept(this, indent + 1);
		}
	}

	#endregion

	#region Private Methods

	private static string DescribeEscape(string escape) => escape switch
	{
		"\\d" => "digit",
		"\\D" => "non-digit",
		"\\w" => "word character",
		"\\W" => "non-word character",
		"\\s" => "whitespace",
		"\\S" => "non-whitespace",
		"\\b" => "word boundary",
		"\\B" => "non-word boundary",
		"\\A" => "start of string",
		"\\z" => "end of string",
		"\\Z" => "end of string before final newline",
		"\\G" => "contiguous match boundary",
		_ => "escape sequence",
	};

	private static string EscapeStringForDisplay(string s)
	{
		StringBuilder sb = new();
		foreach (char c in s)
		{
			switch (c)
			{
				case '\n': sb.Append("\\n"); break;
				case '\r': sb.Append("\\r"); break;
				case '\t': sb.Append("\\t"); break;
				case '\\': sb.Append("\\\\"); break;
				case '"': sb.Append("\\\""); break;
				default: sb.Append(c); break;
			}
		}

		return sb.ToString();
	}

	private static string FormatClassItem(Ast.CharacterClassItem item)
	{
		return item switch
		{
			RangeItem r => $"{QuoteChar(r.From)}..{QuoteChar(r.To)}",
			SingleCharItem s => QuoteChar(s.C),
			CategoryItem c => c.Category,
			_ => "<unknown>",
		};
	}

	private static string FormatSpan(RegexNode node) => $"[{node.Start}..{node.End}]";

	private static string Indent(int n) => new(' ', n * 2);

	private static string QuoteChar(char c)
	{
		return c switch
		{
			'\n' => "'\\n'",
			'\r' => "'\\r'",
			'\t' => "'\\t'",
			'\\' => "'\\\\'",
			'\'' => "'\\''",
			_ when char.IsControl(c) || char.IsSurrogate(c) => $"U+{(int)c:X4}",
			_ => $"'{c}'",
		};
	}

	private void AppendLine(int indent, string text) =>
		this.Builder.AppendLine($"{Indent(indent)}{text}");

	#endregion
}
