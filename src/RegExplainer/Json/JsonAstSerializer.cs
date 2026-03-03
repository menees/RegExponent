namespace RegExplainer.Json;

#region Using Directives

using System.Text.Json;
using System.Text.Json.Serialization;
using RegExplainer.Ast;

#endregion

public static class JsonAstSerializer
{
	#region Private Data Members

	private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

	#endregion

	#region Public Methods

	public static string Serialize(RegexNode node)
	{
		object dto = ToDto(node);
		return JsonSerializer.Serialize(dto, Opts);
	}

	#endregion

	#region Private Methods

	private static object ToDto(RegexNode node)
	{
		return node switch
		{
			SequenceNode s => new
			{
				type = "Sequence",
				start = s.Start,
				end = s.End,
				items = s.Items.ConvertAll(i => ToDto(i)),
			},
			AlternationNode a => new
			{
				type = "Alternation",
				start = a.Start,
				end = a.End,
				alternatives = a.Alternatives.ConvertAll(i => ToDto(i)),
			},
			LiteralNode l => new
			{
				type = "Literal",
				start = l.Start,
				end = l.End,
				text = l.Text,
			},
			DotNode d => new { type = "Dot", start = d.Start, end = d.End },
			AnchorNode an => new { type = "Anchor", start = an.Start, end = an.End, kind = an.Kind.ToString() },
			QuantifierNode q => new
			{
				type = "Quantifier",
				start = q.Start,
				end = q.End,
				min = q.Min,
				max = q.Max,
				lazy = q.IsLazy,
				target = ToDto(q.Target),
			},
			CharacterClassNode cc => ToCharacterClassDto(cc),
			GroupNode g => ToGroupDto(g),
			LookaroundNode ln => new
			{
				type = "Lookaround",
				start = ln.Start,
				end = ln.End,
				kind = ln.Kind.ToString(),
				inner = ToDto(ln.Inner),
			},
			BackreferenceNode br => new { type = "Backreference", start = br.Start, end = br.End, number = br.Number },
			NamedBackreferenceNode nbr => new { type = "NamedBackreference", start = nbr.Start, end = nbr.End, name = nbr.Name },
			ConditionalNode cn => new
			{
				type = "Conditional",
				start = cn.Start,
				end = cn.End,
				condition = cn.Condition,
				trueBranch = ToDto(cn.TrueBranch),
				falseBranch = cn.FalseBranch != null ? ToDto(cn.FalseBranch) : null,
			},
			EscapeNode e => new { type = "Escape", start = e.Start, end = e.End, text = e.EscapeText },
			InlineOptionsNode io => new { type = "InlineOptions", start = io.Start, end = io.End, options = io.Options },
			CommentNode cm => new { type = "Comment", start = cm.Start, end = cm.End, text = cm.Text },
			_ => new { type = node.GetType().Name },
		};
	}

	private static object ToCharacterClassDto(CharacterClassNode cc) => new
	{
		type = "CharacterClass",
		start = cc.Start,
		end = cc.End,
		negated = cc.IsNegated,
		items = cc.Items.ConvertAll(FormatClassItemDto),
		span = new { start = cc.Start, end = cc.End },
	};

	private static object ToGroupDto(GroupNode g) => new
	{
		type = "Group",
		start = g.Start,
		end = g.End,
		capturing = g.IsCapturing,
		name = g.Name,
		inlineOptions = g.InlineOptions,
		inner = ToDto(g.Inner),
		span = new { start = g.Start, end = g.End },
	};

	private static object FormatClassItemDto(CharacterClassItem item)
	{
		return item switch
		{
			RangeItem r => new { kind = "range", from = r.From, to = r.To },
			SingleCharItem s => new { kind = "char", value = s.C },
			CategoryItem c => new { kind = "category", value = c.Category },
			_ => new { kind = "unknown" },
		};
	}

	#endregion
}
