namespace RegExponent;

#region Using Directives

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using RegExplainer.Ast;
using RegExplainer.Visitor;

#endregion

internal sealed class TreeViewExplanationVisitor : ExplanationVisitorBase
{
	#region Private Data Members

	private const double IconFontSize = 10;
	private const double IconPaddingSize = 3;
	private const double IconMarginRight = 6;

	private readonly Stack<(int Indent, TreeViewItem Item)> stack = new();

	#endregion

	#region Public Properties

	public List<TreeViewItem> RootItems { get; } = [];

	#endregion

	#region Protected Methods

	protected override void AppendNode(int indent, string text, ExplainNodeKind nodeKind, RegexNode node)
	{
		(int Start, int Length)? span = node.Start != null && node.End != null && node.End >= node.Start
			? (node.Start.Value, node.End.Value - node.Start.Value)
			: null;

		TreeViewItem item = new()
		{
			Header = CreateHeader(text, nodeKind, node),
			IsExpanded = true,
			Tag = span,
			HorizontalContentAlignment = HorizontalAlignment.Left,
			VerticalContentAlignment = VerticalAlignment.Center,
		};

		// Pop items from the stack that are at the same or deeper indent level.
		while (this.stack.Count > 0 && this.stack.Peek().Indent >= indent)
		{
			this.stack.Pop();
		}

		if (this.stack.Count > 0)
		{
			this.stack.Peek().Item.Items.Add(item);
		}
		else
		{
			this.RootItems.Add(item);
		}

		this.stack.Push((indent, item));
	}

	#endregion

	#region Private Methods

	private static StackPanel CreateHeader(string text, ExplainNodeKind nodeKind, RegexNode node)
	{
		Color color = GetNodeColor(nodeKind);
		string icon = GetNodeIcon(nodeKind);

		StackPanel panel = new() { Orientation = Orientation.Horizontal };

		// Colored icon indicator.
		Border iconBorder = new()
		{
			Background = new SolidColorBrush(color),
			CornerRadius = new CornerRadius(2),
			Padding = new Thickness(IconPaddingSize, 0, IconPaddingSize, 0),
			Margin = new Thickness(0, 0, IconMarginRight, 0),
			VerticalAlignment = VerticalAlignment.Center,
			Child = new TextBlock
			{
				Text = icon,
				Foreground = Brushes.White,
				FontSize = IconFontSize,
				FontWeight = FontWeights.Bold,
				VerticalAlignment = VerticalAlignment.Center,
			},
		};
		panel.Children.Add(iconBorder);

		// Node text with colored label.
		int colonIndex = text.IndexOf(':');
		if (colonIndex > 0 && colonIndex < text.Length - 1)
		{
			string label = text[..(colonIndex + 1)];
			string rest = text[(colonIndex + 1)..];

			panel.Children.Add(new TextBlock
			{
				Text = label,
				FontWeight = FontWeights.SemiBold,
				Style = CreateTextBlockStyle(new SolidColorBrush(color)),
				VerticalAlignment = VerticalAlignment.Center,
			});
			panel.Children.Add(new TextBlock
			{
				Text = rest,
				Style = CreateTextBlockStyle(null),
				VerticalAlignment = VerticalAlignment.Center,
			});
		}
		else
		{
			panel.Children.Add(new TextBlock
			{
				Text = text,
				FontWeight = FontWeights.SemiBold,
				Style = CreateTextBlockStyle(new SolidColorBrush(color)),
				VerticalAlignment = VerticalAlignment.Center,
			});
		}

		// Span indicator (closed-open interval).
		panel.Children.Add(new TextBlock
		{
			Text = $" {FormatSpan(node)}",
			Style = CreateTextBlockStyle(Brushes.Gray),
			VerticalAlignment = VerticalAlignment.Center,
		});

		return panel;
	}

	private static Style CreateTextBlockStyle(Brush? defaultForeground)
	{
		Style style = new(typeof(TextBlock));
		if (defaultForeground != null)
		{
			style.Setters.Add(new Setter(TextBlock.ForegroundProperty, defaultForeground));
		}

		DataTrigger trigger = new()
		{
			Binding = new Binding(nameof(TreeViewItem.IsSelected))
			{
				RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(TreeViewItem), 1),
			},
			Value = true,
		};
		trigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, SystemColors.HighlightTextBrush));
		style.Triggers.Add(trigger);

		return style;
	}

	private static Color GetNodeColor(ExplainNodeKind nodeKind) => nodeKind switch
	{
		ExplainNodeKind.Sequence => Colors.SteelBlue,
		ExplainNodeKind.Alternation => Colors.IndianRed,
		ExplainNodeKind.AlternationBranch => Colors.RosyBrown,
		ExplainNodeKind.Literal => Colors.ForestGreen,
		ExplainNodeKind.Dot => Colors.MediumSeaGreen,
		ExplainNodeKind.Anchor => Colors.MediumPurple,
		ExplainNodeKind.Quantifier or ExplainNodeKind.QuantifierDetail => Colors.DarkOrange,
		ExplainNodeKind.CharacterClass or ExplainNodeKind.CharacterClassDetail => Colors.Teal,
		ExplainNodeKind.Group => Colors.RoyalBlue,
		ExplainNodeKind.Escape => Colors.OliveDrab,
		ExplainNodeKind.Lookaround => Colors.DarkGoldenrod,
		ExplainNodeKind.Backreference or ExplainNodeKind.NamedBackreference => Colors.SaddleBrown,
		ExplainNodeKind.Comment => Colors.Gray,
		ExplainNodeKind.Conditional => Colors.DarkOrchid,
		ExplainNodeKind.ConditionalBranch => Colors.MediumOrchid,
		ExplainNodeKind.InlineOptions => Colors.SlateGray,
		_ => Colors.DimGray,
	};

	private static string GetNodeIcon(ExplainNodeKind nodeKind) => nodeKind switch
	{
		ExplainNodeKind.Sequence => "Seq",
		ExplainNodeKind.Alternation => "Alt",
		ExplainNodeKind.AlternationBranch => "|",
		ExplainNodeKind.Literal => "Abc",
		ExplainNodeKind.Dot => ".",
		ExplainNodeKind.Anchor => "^$",
		ExplainNodeKind.Quantifier => "*+",
		ExplainNodeKind.QuantifierDetail => "\u00D7", // Multiplication Sign
		ExplainNodeKind.CharacterClass => "[ ]",
		ExplainNodeKind.CharacterClassDetail => "...",
		ExplainNodeKind.Group => "( )",
		ExplainNodeKind.Escape => "\\",
		ExplainNodeKind.Lookaround => "?=",
		ExplainNodeKind.Backreference => "\\1",
		ExplainNodeKind.NamedBackreference => "\\k",
		ExplainNodeKind.Comment => "#",
		ExplainNodeKind.Conditional => "?:",
		ExplainNodeKind.ConditionalBranch => "\u2192", // Rightwards Arrow
		ExplainNodeKind.InlineOptions => "(?)",
		_ => "\u00B7", // MIddle Dot
	};

	#endregion
}
