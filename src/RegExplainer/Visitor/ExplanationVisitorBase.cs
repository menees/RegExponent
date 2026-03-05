namespace RegExplainer.Visitor;

#region Using Directives

using RegExplainer.Ast;

#endregion

public abstract class ExplanationVisitorBase : IRegexVisitor
{
	#region Protected Enums

	protected enum ExplainNodeKind
	{
		Sequence,
		Alternation,
		AlternationBranch,
		Literal,
		Dot,
		Anchor,
		Quantifier,
		QuantifierDetail,
		CharacterClass,
		CharacterClassDetail,
		Group,
		Escape,
		Lookaround,
		Backreference,
		NamedBackreference,
		Comment,
		Conditional,
		ConditionalBranch,
		InlineOptions,
	}

	#endregion

	#region Public Methods

	public void VisitAlternation(AlternationNode node, int indent)
	{
		this.AppendNode(indent, $"Alternation (one of {node.Alternatives.Count} branches)", ExplainNodeKind.Alternation, node);
		for (int i = 0; i < node.Alternatives.Count; i++)
		{
			this.AppendNode(indent + 1, $"[{i}] Branch:", ExplainNodeKind.AlternationBranch, node.Alternatives[i]);
			node.Alternatives[i].Accept(this, indent + 2);
		}
	}

	public void VisitAnchor(AnchorNode node, int indent)
	{
		string desc = node.Kind == AnchorKind.Start ? "start of input '^'" : "end of input '$'";
		this.AppendNode(indent, $"Anchor: {desc}", ExplainNodeKind.Anchor, node);
	}

	public void VisitBackreference(BackreferenceNode node, int indent)
	{
		this.AppendNode(indent, $"Backreference: \\{node.Number}", ExplainNodeKind.Backreference, node);
	}

	public void VisitComment(CommentNode node, int indent)
	{
		this.AppendNode(indent, $"Comment: # {node.Text}", ExplainNodeKind.Comment, node);
	}

	public void VisitCharacterClass(CharacterClassNode node, int indent)
	{
		string negated = node.IsNegated ? "^" : string.Empty;
		string text = $"Character class: [{negated}...] ({node.Items.Count} item(s))";
		this.AppendNode(indent, text, ExplainNodeKind.CharacterClass, node);
		string[] parts = [.. node.Items.Select(FormatClassItem)];
		this.AppendNode(indent + 1, $"Contents: {string.Join(", ", parts)}", ExplainNodeKind.CharacterClassDetail, node.Contents);
	}

	public void VisitConditional(ConditionalNode node, int indent)
	{
		this.AppendNode(indent, $"Conditional: if ({node.Condition})", ExplainNodeKind.Conditional, node);
		this.AppendNode(indent + 1, "True branch:", ExplainNodeKind.ConditionalBranch, node.TrueBranch);
		node.TrueBranch.Accept(this, indent + 2);
		if (node.FalseBranch != null)
		{
			this.AppendNode(indent + 1, "False branch:", ExplainNodeKind.ConditionalBranch, node.FalseBranch);
			node.FalseBranch.Accept(this, indent + 2);
		}
	}

	public void VisitDot(DotNode node, int indent)
	{
		this.AppendNode(indent, "Dot: matches any character (except newline by default)", ExplainNodeKind.Dot, node);
	}

	public void VisitEscape(EscapeNode node, int indent)
	{
		string desc = DescribeEscape(node.EscapeText);
		this.AppendNode(indent, $"Escape: {node.EscapeText} ({desc})", ExplainNodeKind.Escape, node);
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

		this.AppendNode(indent, $"Group: {kind}", ExplainNodeKind.Group, node);
		node.Inner.Accept(this, indent + 1);
	}

	public void VisitInlineOptions(InlineOptionsNode node, int indent)
	{
		this.AppendNode(indent, $"Inline options: (?{node.Options})", ExplainNodeKind.InlineOptions, node);
	}

	public void VisitLiteral(LiteralNode node, int indent)
	{
		string disp = node.Text.Length == 1 ? QuoteChar(node.Text[0]) : $"\"{EscapeStringForDisplay(node.Text)}\"";
		this.AppendNode(indent, $"Literal: {disp}", ExplainNodeKind.Literal, node);
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
		this.AppendNode(indent, desc, ExplainNodeKind.Lookaround, node);
		node.Inner.Accept(this, indent + 1);
	}

	public void VisitNamedBackreference(NamedBackreferenceNode node, int indent)
	{
		this.AppendNode(indent, $"Named backreference: \\k<{node.Name}>", ExplainNodeKind.NamedBackreference, node);
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
		this.AppendNode(indent + 1, $"(repeated {range}, {mode})", ExplainNodeKind.QuantifierDetail, node);
	}

	public void VisitSequence(SequenceNode node, int indent)
	{
		this.AppendNode(indent, $"Sequence ({node.Items.Count} item(s))", ExplainNodeKind.Sequence, node);
		foreach (RegexNode child in node.Items)
		{
			child.Accept(this, indent + 1);
		}
	}

	#endregion

	#region Protected Methods

	protected static string FormatSpan(RegexNode node) => $"[{node.Start},{node.End})";

	protected abstract void AppendNode(int indent, string text, ExplainNodeKind nodeKind, RegexNode node);

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
		System.Text.StringBuilder sb = new();
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

	private static string FormatClassItem(CharacterClassItem item)
	{
		return item switch
		{
			RangeItem r => $"{QuoteChar(r.From)}..{QuoteChar(r.To)}",
			SingleCharItem s => QuoteChar(s.C),
			CategoryItem c => c.Category,
			_ => "<unknown>",
		};
	}

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

	#endregion
}
