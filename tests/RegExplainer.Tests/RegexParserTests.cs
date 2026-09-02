namespace RegExplainer.Tests;

#region Using Directives

using RegExplainer.Ast;
using RegExplainer.Json;
using RegExplainer.Parser;

#endregion

[TestClass]
public class RegexParserTests
{
	#region Public Methods

	[TestMethod]
	public void ParsesSimpleLiteral()
	{
		RegexParser p = new("abc");
		RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		v.Builder.ToString().ShouldContain("Literal");
	}

	[TestMethod]
	public void ParsesQuantifier()
	{
		RegexParser p = new("a{2,3}");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Quantifier");
	}

	[TestMethod]
	public void ParsesNamedGroupAndBackref()
	{
		RegexParser p = new("""(?<name>abc)\k<name>""");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("NamedBackreference");
		json.ShouldContain("Group");
	}

	[TestMethod]
	public void ParsesPosixClass()
	{
		RegexParser p = new("[[:digit:]]");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.Contains("[:digit:]").ShouldBeTrue();
	}

	[TestMethod]
	public void ParsesConditional()
	{
		RegexParser p = new("(?(1)ab|cd)");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Conditional");
	}

	[TestMethod]
	public void ParsesLookaround()
	{
		RegexParser p = new("""(?<=$)\d+(?= USD)""");
		RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		v.Builder.ToString().ShouldContain("lookbehind", Case.Insensitive);
	}

	[TestMethod]
	public void QuantifierSpanEncompassesTarget()
	{
		// In \d+, the quantifier span should start at \d (pos 0), not at + (pos 2)
		RegexParser p = new("""\d+""");
		RegexNode ast = p.Parse();
		QuantifierNode quant = ast.ShouldBeOfType<QuantifierNode>();
		quant.Start.ShouldBe(0);
		quant.End.ShouldBe(3);
		quant.Target.Start.ShouldBe(0);
		quant.Target.End.ShouldBe(2);
	}

	[TestMethod]
	public void ParsesNonCapturingGroup()
	{
		RegexParser p = new("(?:abc)");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Group");
		json.ShouldContain("capturing");
	}

	[TestMethod]
	public void ParsesNamedGroupPStyle()
	{
		RegexParser p = new("(?P<name>xyz)");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Group");
		json.ShouldContain("name");
	}

	[TestMethod]
	public void ParsesInlineOptionsInGroup()
	{
		RegexParser p = new("(?i:abc)");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("inlineOptions");
	}

	[TestMethod]
	public void ParsesInlineCommentsSeparatelyFromInlineOptions()
	{
		const string pattern = "(?i)(?#Start case-insensitive)t(?-i)(?#Then be case-sensitive)est";
		RegexParser p = new(pattern);
		SequenceNode sequence = p.Parse().ShouldBeOfType<SequenceNode>();

		sequence.Items.Count.ShouldBe(6);
		sequence.Items[0].ShouldBeOfType<InlineOptionsNode>().Options.ShouldBe("i");
		CommentNode firstComment = sequence.Items[1].ShouldBeOfType<CommentNode>();
		firstComment.Text.ShouldBe("Start case-insensitive");
		firstComment.IsInline.ShouldBeTrue();
		firstComment.Start.ShouldBe(4);
		firstComment.End.ShouldBe(30);
		sequence.Items[2].ShouldBeOfType<LiteralNode>().Text.ShouldBe("t");
		sequence.Items[3].ShouldBeOfType<InlineOptionsNode>().Options.ShouldBe("-i");
		CommentNode secondComment = sequence.Items[4].ShouldBeOfType<CommentNode>();
		secondComment.Text.ShouldBe("Then be case-sensitive");
		secondComment.IsInline.ShouldBeTrue();
		sequence.Items[5].ShouldBeOfType<LiteralNode>().Text.ShouldBe("est");

		Visitor.ExplanationVisitor visitor = new();
		sequence.Accept(visitor, 0);
		string explanation = visitor.Builder.ToString();
		explanation.ShouldContain("Inline comment: (?#Start case-insensitive) [4,30)");
		explanation.ShouldNotContain("s: enables singleline mode");
	}

	[TestMethod]
	public void ParsesPositiveAndNegativeLookahead()
	{
		RegexParser p = new("foo(?=bar)baz(?!qux)");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Lookaround");
		json.ShouldContain("PositiveLookahead");
		json.ShouldContain("NegativeLookahead");
	}

	[TestMethod]
	public void ParsesPositiveAndNegativeLookbehind()
	{
		RegexParser p = new("(?<=pre)mid(?<!nop)");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Lookaround");
		json.ShouldContain("PositiveLookbehind");
		json.ShouldContain("NegativeLookbehind");
	}

	[TestMethod]
	public void WordBoundaryIsNotLiteral()
	{
		// \b is a word boundary assertion, not literal 'b'
		RegexParser p = new("""\b\w+\b""");
		RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		string explanation = v.Builder.ToString();
		explanation.ShouldContain("Escape");
		explanation.ShouldContain("""\b""");
		explanation.ShouldNotContain("Literal: 'b'");
	}

	[TestMethod]
	public void NonWordBoundaryIsNotLiteral()
	{
		RegexParser p = new("""\B""");
		RegexNode ast = p.Parse();
		ast.ShouldBeOfType<EscapeNode>();
	}

	[TestMethod]
	public void StringAnchorsAreNotLiterals()
	{
		// \A is start-of-string, \z is end-of-string — not literal 'A'/'z'
		RegexParser p = new("""\Ahello\z""");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Escape");
		json.ShouldNotContain("""
			"text": "A"
			""");
		json.ShouldNotContain("""
			"text": "z"
			""");
	}

	[TestMethod]
	public void DotAnchorSpanPositionsAreCorrect()
	{
		// ^.$ — each single-char token should have a width-1 span at its position
		RegexParser p = new("^.$");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(3);

		AnchorNode caret = seq.Items[0].ShouldBeOfType<AnchorNode>();
		caret.Start.ShouldBe(0);
		caret.End.ShouldBe(1);

		DotNode dot = seq.Items[1].ShouldBeOfType<DotNode>();
		dot.Start.ShouldBe(1);
		dot.End.ShouldBe(2);

		AnchorNode dollar = seq.Items[2].ShouldBeOfType<AnchorNode>();
		dollar.Start.ShouldBe(2);
		dollar.End.ShouldBe(3);
	}

	[TestMethod]
	public void EscapeTextIsNotDoubleEscaped()
	{
		// \d should store EscapeText as "\d", not "\\d"
		RegexParser p = new("""\d""");
		RegexNode ast = p.Parse();
		EscapeNode esc = ast.ShouldBeOfType<EscapeNode>();
		esc.EscapeText.ShouldBe("""\d""");
		esc.EscapeText.Length.ShouldBe(2); // one backslash + 'd'
	}

	[TestMethod]
	public void ConditionalSeparatesTrueAndFalseBranches()
	{
		// (?(1)ab|cd) — true branch is "ab", false branch is "cd"
		RegexParser p = new("(?(1)ab|cd)");
		RegexNode ast = p.Parse();
		ConditionalNode cond = ast.ShouldBeOfType<ConditionalNode>();
		cond.Condition.ShouldBe("1");

		LiteralNode trueLit = cond.TrueBranch.ShouldBeOfType<LiteralNode>();
		trueLit.Text.ShouldBe("ab");

		cond.FalseBranch.ShouldNotBeNull();
		LiteralNode falseLit = cond.FalseBranch!.ShouldBeOfType<LiteralNode>();
		falseLit.Text.ShouldBe("cd");
	}

	[TestMethod]
	public void AlternationSpanCoversAllBranches()
	{
		// a|b|c — alternation span should be [0..5], not [5..5]
		RegexParser p = new("a|b|c");
		RegexNode ast = p.Parse();
		AlternationNode alt = ast.ShouldBeOfType<AlternationNode>();
		alt.Start.ShouldBe(0);
		alt.End.ShouldBe(5);
		alt.Alternatives.Count.ShouldBe(3);
	}

	[TestMethod]
	public void AlternationSpanCorrectInsideGroup()
	{
		// (a|b) — inner alternation should span [1..4]
		RegexParser p = new("(a|b)");
		RegexNode ast = p.Parse();
		GroupNode group = ast.ShouldBeOfType<GroupNode>();
		AlternationNode alt = group.Inner.ShouldBeOfType<AlternationNode>();
		alt.Start.ShouldBe(1);
		alt.End.ShouldBe(4);
	}

	[TestMethod]
	public void BackslashZeroIsNullCharNotBackreference()
	{
		// \0 is the null character, not a backreference to group 0
		RegexParser p = new("""\0""");
		RegexNode ast = p.Parse();
		LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("\0");
	}

	[TestMethod]
	public void BackslashGIsEscapeNotLiteral()
	{
		// \G matches at the position where the previous match ended
		RegexParser p = new("""\G""");
		RegexNode ast = p.Parse();
		EscapeNode esc = ast.ShouldBeOfType<EscapeNode>();
		esc.EscapeText.ShouldBe("""\G""");
	}

	[TestMethod]
	public void HexEscapeProducesCorrectLiteral()
	{
		RegexParser p = new("""\x41""");
		RegexNode ast = p.Parse();
		LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("A");
	}

	[TestMethod]
	public void UnicodeEscapeProducesCorrectLiteral()
	{
		RegexParser p = new("""\u0042""");
		RegexNode ast = p.Parse();
		LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("B");
	}

	[TestMethod]
	public void ControlCharEscapeProducesCorrectLiteral()
	{
		// \cA = Ctrl-A = U+0001
		RegexParser p = new("""\cA""");
		RegexNode ast = p.Parse();
		LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("\x01");
	}

	[TestMethod]
	public void FormFeedAndVerticalTabAreNotLiterals()
	{
		RegexParser p = new("""\f\v""");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		LiteralNode ff = seq.Items[0].ShouldBeOfType<LiteralNode>();
		ff.Text.ShouldBe("\f");
		LiteralNode vt = seq.Items[1].ShouldBeOfType<LiteralNode>();
		vt.Text.ShouldBe("\v");
	}

	[TestMethod]
	public void UnrecognizedLetterEscapeThrows()
	{
		RegexParser p = new("""\q""");
		Should.Throw<RegexParseException>(p.Parse);
	}

	[TestMethod]
	public void NegatedCharacterClassOutputsNegatedDescription()
	{
		RegexParser p = new("[^a-z]");
		RegexNode ast = p.Parse();

		MockExplanationVisitor mockVisitor = new();
		ast.Accept(mockVisitor, 0);

		mockVisitor.NodesList.Count.ShouldBeGreaterThan(0);
		mockVisitor.NodesList[0].Text.ShouldStartWith("Negated character class");
	}

	[TestMethod]
	public void BackslashPWithoutBraceThrows()
	{
		RegexParser p = new("""\p""");
		Should.Throw<RegexParseException>(p.Parse);
	}

	[TestMethod]
	public void BackslashKWithoutAngleBracketThrows()
	{
		RegexParser p = new("""\k""");
		Should.Throw<RegexParseException>(p.Parse);
	}

	[TestMethod]
	public void UnterminatedGroupThrowsProperError()
	{
		RegexParser p = new("(?");
		Should.Throw<RegexParseException>(p.Parse);
	}

	[TestMethod]
	public void UnterminatedQuantifierThrowsProperError()
	{
		RegexParser p = new("a{3");
		Should.Throw<RegexParseException>(p.Parse);
	}

	[TestMethod]
	public void EscapedMetacharsStillWork()
	{
		// Escaped punctuation should still produce literals
		RegexParser p = new("""\.\*\+\?\(\)""");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(6);
		seq.Items[0].ShouldBeOfType<LiteralNode>().Text.ShouldBe(".");
		seq.Items[1].ShouldBeOfType<LiteralNode>().Text.ShouldBe("*");
	}

	[TestMethod]
	public void HexEscapeInCharacterClass()
	{
		RegexParser p = new("""[\x41]""");
		RegexNode ast = p.Parse();
		CharacterClassNode cc = ast.ShouldBeOfType<CharacterClassNode>();
		cc.Items.Count.ShouldBe(1);
		SingleCharItem item = cc.Items[0].ShouldBeOfType<SingleCharItem>();
		item.C.ShouldBe('A');
	}

	[TestMethod]
	public void BackslashZeroInCharacterClassIsNullChar()
	{
		RegexParser p = new("""[\0]""");
		RegexNode ast = p.Parse();
		CharacterClassNode cc = ast.ShouldBeOfType<CharacterClassNode>();
		cc.Items.Count.ShouldBe(1);
		SingleCharItem item = cc.Items[0].ShouldBeOfType<SingleCharItem>();
		item.C.ShouldBe('\0');
	}

	[TestMethod]
	public void OctalEscapeThrows()
	{
		// \012 should not silently misparse — it's an unsupported octal escape
		RegexParser p = new("""\012""");
		Should.Throw<RegexParseException>(p.Parse);
	}

	[TestMethod]
	public void TryParseReturnsTrueForValidPattern()
	{
		RegexParser p = new("""\d+""");
		bool success = p.TryParse(out RegexNode? ast, out RegexParseException? error);
		success.ShouldBeTrue();
		ast.ShouldNotBeNull();
		error.ShouldBeNull();
	}

	[TestMethod]
	public void TryParseReturnsFalseForInvalidPattern()
	{
		RegexParser p = new("""\q""");
		bool success = p.TryParse(out RegexNode? ast, out RegexParseException? error);
		success.ShouldBeFalse();
		ast.ShouldBeNull();
		error.ShouldNotBeNull();
		error.Message.ShouldContain("Unrecognized escape");
		error.Position.ShouldBe(2);
	}

	[TestMethod]
	public void ParsesStandaloneInlineOptions()
	{
		RegexParser p = new("(?n)abc");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);

		InlineOptionsNode opts = seq.Items[0].ShouldBeOfType<InlineOptionsNode>();
		opts.Options.ShouldBe("n");
		opts.Start.ShouldBe(0);
		opts.End.ShouldBe(4);

		seq.Items[1].ShouldBeOfType<LiteralNode>().Text.ShouldBe("abc");
	}

	[TestMethod]
	public void ParsesStandaloneInlineOptionsMultipleFlags()
	{
		RegexParser p = new("(?i-s)test");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);

		InlineOptionsNode opts = seq.Items[0].ShouldBeOfType<InlineOptionsNode>();
		opts.Options.ShouldBe("i-s");
	}

	[TestMethod]
	public void ParsesStandaloneInlineOptionsSingleFlag()
	{
		RegexParser p = new("(?i)");
		RegexNode ast = p.Parse();
		InlineOptionsNode opts = ast.ShouldBeOfType<InlineOptionsNode>();
		opts.Options.ShouldBe("i");
	}

	[TestMethod]
	public void StandaloneInlineOptionsJsonContainsInlineOptions()
	{
		RegexParser p = new("(?n)abc");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("InlineOptions");
		json.ShouldContain("\"n\"");
	}

	[TestMethod]
	public void StandaloneInlineOptionsExplanationContainsOptions()
	{
		RegexParser p = new("(?nx)abc");
		RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		string explanation = v.Builder.ToString();
		explanation.ShouldContain("Inline options");
		explanation.ShouldContain("(?nx)");
		explanation.ShouldContain("  - n: enables explicit capture; unnamed groups do not capture [2,3)");
		explanation.ShouldContain("  - x: ignores unescaped whitespace and enables # comments [3,4)");
	}

	[TestMethod]
	public void StandaloneInlineOptionsExplanationDescribesNegatedOptions()
	{
		RegexParser p = new("(?i-msx)");
		RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		string explanation = v.Builder.ToString();
		explanation.ShouldContain("  - i: enables case-insensitive matching [2,3)");
		explanation.ShouldContain("  - -: disables the options that follow [3,4)");
		explanation.ShouldContain("  - m: disables multiline mode; ^ and $ match only input boundaries [4,5)");
		explanation.ShouldContain("  - s: disables singleline mode; . does not match newlines [5,6)");
		explanation.ShouldContain("  - x: treats unescaped whitespace and # as pattern characters [6,7)");
	}

	[TestMethod]
	public void ParsesPatternWithStandaloneInlineOptionsAndNamedGroups()
	{
		// Simplified version of the original bug report pattern: (?n) followed by named captures.
		RegexParser p = new("""(?n)x:(?<Long>-?\d+),y:(?<Lat>-?\d+)""");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("InlineOptions");
		json.ShouldContain("Long");
		json.ShouldContain("Lat");
	}

	[TestMethod]
	public void LiteralBraceNotMistakenForQuantifier()
	{
		// { not followed by a digit is a literal, not a quantifier.
		RegexParser p = new("a{b}");
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldNotContain("Quantifier");
		json.ShouldContain("Literal");
	}

	[TestMethod]
	public void BraceQuantifierStillWorks()
	{
		// {3} after a literal should still parse as a quantifier.
		RegexParser p = new("a{3}");
		RegexNode ast = p.Parse();
		QuantifierNode quant = ast.ShouldBeOfType<QuantifierNode>();
		quant.Min.ShouldBe(3);
		quant.Max.ShouldBe(3);
	}

	[TestMethod]
	public void BraceRangeQuantifierStillWorks()
	{
		RegexParser p = new("a{2,5}");
		RegexNode ast = p.Parse();
		QuantifierNode quant = ast.ShouldBeOfType<QuantifierNode>();
		quant.Min.ShouldBe(2);
		quant.Max.ShouldBe(5);
	}

	[TestMethod]
	public void ParsesOriginalBugReportPattern()
	{
		// Full pattern from the original bug report.
		const string pattern = """(?n)"geometry":{"x":(?<Long>-?\d+(\.\d+)?),"y":(?<Lat>-?\d+(\.\d+)?)(,"z":(?<Alt>-?\d+(\.\d+)?))?}""";
		RegexParser p = new(pattern);
		RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("InlineOptions");
		json.ShouldContain("Long");
		json.ShouldContain("Lat");
		json.ShouldContain("Alt");
	}

	[TestMethod]
	public void ExplicitCaptureOptionMakesUnnamedGroupNonCapturing()
	{
		// With RegexOptions.ExplicitCapture, unnamed (...) groups become non-capturing.
		RegexParser p = new("(abc)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		RegexNode ast = p.Parse();
		GroupNode group = ast.ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeFalse();
	}

	[TestMethod]
	public void ExplicitCaptureOptionPreservesNamedGroupCapturing()
	{
		// Named groups should still capture even with ExplicitCapture.
		RegexParser p = new("(?<name>abc)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		RegexNode ast = p.Parse();
		GroupNode group = ast.ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
		group.Name.ShouldBe("name");
	}

	[TestMethod]
	public void InlineOptionNMakesSubsequentUnnamedGroupNonCapturing()
	{
		// (?n) followed by (...) — the unnamed group should become non-capturing.
		RegexParser p = new("(?n)(abc)");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);
		seq.Items[0].ShouldBeOfType<InlineOptionsNode>().Options.ShouldBe("n");
		GroupNode group = seq.Items[1].ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeFalse();
	}

	[TestMethod]
	public void InlineOptionNDoesNotAffectNamedGroups()
	{
		// (?n) followed by (?<name>...) — named groups should still capture.
		RegexParser p = new("(?n)(?<name>abc)");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);
		GroupNode group = seq.Items[1].ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
		group.Name.ShouldBe("name");
	}

	[TestMethod]
	public void ScopedInlineOptionNIsRestoredAfterGroup()
	{
		// (?n:(a))(b) — inside (?n:...) the unnamed group is non-capturing,
		// but outside it the unnamed group should be capturing again.
		RegexParser p = new("(?n:(a))(b)");
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);

		// The scoped group (?n:...) itself is non-capturing.
		GroupNode scopedGroup = seq.Items[0].ShouldBeOfType<GroupNode>();
		scopedGroup.IsCapturing.ShouldBeFalse();

		// Inner (a) should be non-capturing because of (?n:...).
		GroupNode innerGroup = scopedGroup.Inner.ShouldBeOfType<GroupNode>();
		innerGroup.IsCapturing.ShouldBeFalse();

		// Outer (b) should be capturing because n scope has ended.
		GroupNode outerGroup = seq.Items[1].ShouldBeOfType<GroupNode>();
		outerGroup.IsCapturing.ShouldBeTrue();
	}

	[TestMethod]
	public void InlineOptionMinusNReEnablesCapturing()
	{
		// With ExplicitCapture, (?-n) should re-enable unnamed capturing.
		RegexParser p = new("(?-n)(abc)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		RegexNode ast = p.Parse();
		SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);
		seq.Items[0].ShouldBeOfType<InlineOptionsNode>().Options.ShouldBe("-n");
		GroupNode group = seq.Items[1].ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
	}

	[TestMethod]
	public void WithoutExplicitCaptureUnnamedGroupIsCapturing()
	{
		// Baseline: without ExplicitCapture, (...) is capturing.
		RegexParser p = new("(abc)");
		RegexNode ast = p.Parse();
		GroupNode group = ast.ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
	}

	#endregion

	#region Private Types

	private sealed class MockExplanationVisitor : Visitor.ExplanationVisitorBase
	{
		#region Constructors

		public MockExplanationVisitor()
		{
		}

		#endregion

		#region Public Properties

		public List<(int Indent, string Text, RegexNode Node)> NodesList { get; } = [];

		#endregion

		#region Protected Methods

		protected override void AppendNode(int indent, string text, ExplainNodeKind nodeKind, RegexNode node)
		{
			this.NodesList.Add((indent, text, node));
		}

		#endregion
	}

	#endregion
}
