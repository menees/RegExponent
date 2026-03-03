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
		Ast.RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		v.Builder.ToString().ShouldContain("Literal");
	}

	[TestMethod]
	public void ParsesQuantifier()
	{
		RegexParser p = new("a{2,3}");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Quantifier");
	}

	[TestMethod]
	public void ParsesNamedGroupAndBackref()
	{
		RegexParser p = new("""(?<name>abc)\k<name>""");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("NamedBackreference");
		json.ShouldContain("Group");
	}

	[TestMethod]
	public void ParsesPosixClass()
	{
		RegexParser p = new("[[:digit:]]");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.Contains("[:digit:]").ShouldBeTrue();
	}

	[TestMethod]
	public void ParsesConditional()
	{
		RegexParser p = new("(?(1)ab|cd)");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Conditional");
	}

	[TestMethod]
	public void ParsesLookaround()
	{
		RegexParser p = new("""(?<=$)\d+(?= USD)""");
		Ast.RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		v.Builder.ToString().ShouldContain("lookbehind", Case.Insensitive);
	}

	[TestMethod]
	public void QuantifierSpanEncompassesTarget()
	{
		// In \d+, the quantifier span should start at \d (pos 0), not at + (pos 2)
		RegexParser p = new("""\d+""");
		Ast.RegexNode ast = p.Parse();
		Ast.QuantifierNode quant = ast.ShouldBeOfType<QuantifierNode>();
		quant.Start.ShouldBe(0);
		quant.End.ShouldBe(3);
		quant.Target.Start.ShouldBe(0);
		quant.Target.End.ShouldBe(2);
	}

	[TestMethod]
	public void ParsesNonCapturingGroup()
	{
		RegexParser p = new("(?:abc)");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Group");
		json.ShouldContain("capturing");
	}

	[TestMethod]
	public void ParsesNamedGroupPStyle()
	{
		RegexParser p = new("(?P<name>xyz)");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Group");
		json.ShouldContain("name");
	}

	[TestMethod]
	public void ParsesInlineOptionsInGroup()
	{
		RegexParser p = new("(?i:abc)");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("inlineOptions");
	}

	[TestMethod]
	public void ParsesPositiveAndNegativeLookahead()
	{
		RegexParser p = new("foo(?=bar)baz(?!qux)");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("Lookaround");
		json.ShouldContain("PositiveLookahead");
		json.ShouldContain("NegativeLookahead");
	}

	[TestMethod]
	public void ParsesPositiveAndNegativeLookbehind()
	{
		RegexParser p = new("(?<=pre)mid(?<!nop)");
		Ast.RegexNode ast = p.Parse();
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
		Ast.RegexNode ast = p.Parse();
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
		Ast.RegexNode ast = p.Parse();
		ast.ShouldBeOfType<EscapeNode>();
	}

	[TestMethod]
	public void StringAnchorsAreNotLiterals()
	{
		// \A is start-of-string, \z is end-of-string — not literal 'A'/'z'
		RegexParser p = new("""\Ahello\z""");
		Ast.RegexNode ast = p.Parse();
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
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(3);

		Ast.AnchorNode caret = seq.Items[0].ShouldBeOfType<AnchorNode>();
		caret.Start.ShouldBe(0);
		caret.End.ShouldBe(1);

		Ast.DotNode dot = seq.Items[1].ShouldBeOfType<DotNode>();
		dot.Start.ShouldBe(1);
		dot.End.ShouldBe(2);

		Ast.AnchorNode dollar = seq.Items[2].ShouldBeOfType<AnchorNode>();
		dollar.Start.ShouldBe(2);
		dollar.End.ShouldBe(3);
	}

	[TestMethod]
	public void EscapeTextIsNotDoubleEscaped()
	{
		// \d should store EscapeText as "\d", not "\\d"
		RegexParser p = new("""\d""");
		Ast.RegexNode ast = p.Parse();
		Ast.EscapeNode esc = ast.ShouldBeOfType<EscapeNode>();
		esc.EscapeText.ShouldBe("""\d""");
		esc.EscapeText.Length.ShouldBe(2); // one backslash + 'd'
	}

	[TestMethod]
	public void ConditionalSeparatesTrueAndFalseBranches()
	{
		// (?(1)ab|cd) — true branch is "ab", false branch is "cd"
		RegexParser p = new("(?(1)ab|cd)");
		Ast.RegexNode ast = p.Parse();
		Ast.ConditionalNode cond = ast.ShouldBeOfType<ConditionalNode>();
		cond.Condition.ShouldBe("1");

		Ast.LiteralNode trueLit = cond.TrueBranch.ShouldBeOfType<LiteralNode>();
		trueLit.Text.ShouldBe("ab");

		cond.FalseBranch.ShouldNotBeNull();
		Ast.LiteralNode falseLit = cond.FalseBranch!.ShouldBeOfType<LiteralNode>();
		falseLit.Text.ShouldBe("cd");
	}

	[TestMethod]
	public void AlternationSpanCoversAllBranches()
	{
		// a|b|c — alternation span should be [0..5], not [5..5]
		RegexParser p = new("a|b|c");
		Ast.RegexNode ast = p.Parse();
		Ast.AlternationNode alt = ast.ShouldBeOfType<AlternationNode>();
		alt.Start.ShouldBe(0);
		alt.End.ShouldBe(5);
		alt.Alternatives.Count.ShouldBe(3);
	}

	[TestMethod]
	public void AlternationSpanCorrectInsideGroup()
	{
		// (a|b) — inner alternation should span [1..4]
		RegexParser p = new("(a|b)");
		Ast.RegexNode ast = p.Parse();
		Ast.GroupNode group = ast.ShouldBeOfType<GroupNode>();
		Ast.AlternationNode alt = group.Inner.ShouldBeOfType<AlternationNode>();
		alt.Start.ShouldBe(1);
		alt.End.ShouldBe(4);
	}

	[TestMethod]
	public void BackslashZeroIsNullCharNotBackreference()
	{
		// \0 is the null character, not a backreference to group 0
		RegexParser p = new("""\0""");
		Ast.RegexNode ast = p.Parse();
		Ast.LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("\0");
	}

	[TestMethod]
	public void BackslashGIsEscapeNotLiteral()
	{
		// \G matches at the position where the previous match ended
		RegexParser p = new("""\G""");
		Ast.RegexNode ast = p.Parse();
		Ast.EscapeNode esc = ast.ShouldBeOfType<EscapeNode>();
		esc.EscapeText.ShouldBe("""\G""");
	}

	[TestMethod]
	public void HexEscapeProducesCorrectLiteral()
	{
		RegexParser p = new("""\x41""");
		Ast.RegexNode ast = p.Parse();
		Ast.LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("A");
	}

	[TestMethod]
	public void UnicodeEscapeProducesCorrectLiteral()
	{
		RegexParser p = new("""\u0042""");
		Ast.RegexNode ast = p.Parse();
		Ast.LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("B");
	}

	[TestMethod]
	public void ControlCharEscapeProducesCorrectLiteral()
	{
		// \cA = Ctrl-A = U+0001
		RegexParser p = new("""\cA""");
		Ast.RegexNode ast = p.Parse();
		Ast.LiteralNode lit = ast.ShouldBeOfType<LiteralNode>();
		lit.Text.ShouldBe("\x01");
	}

	[TestMethod]
	public void FormFeedAndVerticalTabAreNotLiterals()
	{
		RegexParser p = new("""\f\v""");
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		Ast.LiteralNode ff = seq.Items[0].ShouldBeOfType<LiteralNode>();
		ff.Text.ShouldBe("\f");
		Ast.LiteralNode vt = seq.Items[1].ShouldBeOfType<LiteralNode>();
		vt.Text.ShouldBe("\v");
	}

	[TestMethod]
	public void UnrecognizedLetterEscapeThrows()
	{
		RegexParser p = new("""\q""");
		Should.Throw<RegexParseException>(() => p.Parse());
	}

	[TestMethod]
	public void BackslashPWithoutBraceThrows()
	{
		RegexParser p = new("""\p""");
		Should.Throw<RegexParseException>(() => p.Parse());
	}

	[TestMethod]
	public void BackslashKWithoutAngleBracketThrows()
	{
		RegexParser p = new("""\k""");
		Should.Throw<RegexParseException>(() => p.Parse());
	}

	[TestMethod]
	public void UnterminatedGroupThrowsProperError()
	{
		RegexParser p = new("(?");
		Should.Throw<RegexParseException>(() => p.Parse());
	}

	[TestMethod]
	public void UnterminatedQuantifierThrowsProperError()
	{
		RegexParser p = new("a{3");
		Should.Throw<RegexParseException>(() => p.Parse());
	}

	[TestMethod]
	public void EscapedMetacharsStillWork()
	{
		// Escaped punctuation should still produce literals
		RegexParser p = new("""\.\*\+\?\(\)""");
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(6);
		seq.Items[0].ShouldBeOfType<LiteralNode>().Text.ShouldBe(".");
		seq.Items[1].ShouldBeOfType<LiteralNode>().Text.ShouldBe("*");
	}

	[TestMethod]
	public void HexEscapeInCharacterClass()
	{
		RegexParser p = new("""[\x41]""");
		Ast.RegexNode ast = p.Parse();
		Ast.CharacterClassNode cc = ast.ShouldBeOfType<CharacterClassNode>();
		cc.Items.Count.ShouldBe(1);
		Ast.SingleCharItem item = cc.Items[0].ShouldBeOfType<SingleCharItem>();
		item.C.ShouldBe('A');
	}

	[TestMethod]
	public void BackslashZeroInCharacterClassIsNullChar()
	{
		RegexParser p = new("""[\0]""");
		Ast.RegexNode ast = p.Parse();
		Ast.CharacterClassNode cc = ast.ShouldBeOfType<CharacterClassNode>();
		cc.Items.Count.ShouldBe(1);
		Ast.SingleCharItem item = cc.Items[0].ShouldBeOfType<SingleCharItem>();
		item.C.ShouldBe('\0');
	}

	[TestMethod]
	public void OctalEscapeThrows()
	{
		// \012 should not silently misparse — it's an unsupported octal escape
		RegexParser p = new("""\012""");
		Should.Throw<RegexParseException>(() => p.Parse());
	}

	[TestMethod]
	public void TryParseReturnsTrueForValidPattern()
	{
		RegexParser p = new("""\d+""");
		bool success = p.TryParse(out Ast.RegexNode? ast, out RegexParseException? error);
		success.ShouldBeTrue();
		ast.ShouldNotBeNull();
		error.ShouldBeNull();
	}

	[TestMethod]
	public void TryParseReturnsFalseForInvalidPattern()
	{
		RegexParser p = new("""\q""");
		bool success = p.TryParse(out Ast.RegexNode? ast, out RegexParseException? error);
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
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);

		Ast.InlineOptionsNode opts = seq.Items[0].ShouldBeOfType<InlineOptionsNode>();
		opts.Options.ShouldBe("n");
		opts.Start.ShouldBe(0);
		opts.End.ShouldBe(4);

		seq.Items[1].ShouldBeOfType<LiteralNode>().Text.ShouldBe("abc");
	}

	[TestMethod]
	public void ParsesStandaloneInlineOptionsMultipleFlags()
	{
		RegexParser p = new("(?i-s)test");
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);

		Ast.InlineOptionsNode opts = seq.Items[0].ShouldBeOfType<InlineOptionsNode>();
		opts.Options.ShouldBe("i-s");
	}

	[TestMethod]
	public void ParsesStandaloneInlineOptionsSingleFlag()
	{
		RegexParser p = new("(?i)");
		Ast.RegexNode ast = p.Parse();
		Ast.InlineOptionsNode opts = ast.ShouldBeOfType<InlineOptionsNode>();
		opts.Options.ShouldBe("i");
	}

	[TestMethod]
	public void StandaloneInlineOptionsJsonContainsInlineOptions()
	{
		RegexParser p = new("(?n)abc");
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldContain("InlineOptions");
		json.ShouldContain("\"n\"");
	}

	[TestMethod]
	public void StandaloneInlineOptionsExplanationContainsOptions()
	{
		RegexParser p = new("(?n)abc");
		Ast.RegexNode ast = p.Parse();
		Visitor.ExplanationVisitor v = new();
		ast.Accept(v, 0);
		string explanation = v.Builder.ToString();
		explanation.ShouldContain("Inline options");
		explanation.ShouldContain("(?n)");
	}

	[TestMethod]
	public void ParsesPatternWithStandaloneInlineOptionsAndNamedGroups()
	{
		// Simplified version of the original bug report pattern: (?n) followed by named captures.
		RegexParser p = new("""(?n)x:(?<Long>-?\d+),y:(?<Lat>-?\d+)""");
		Ast.RegexNode ast = p.Parse();
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
		Ast.RegexNode ast = p.Parse();
		string json = JsonAstSerializer.Serialize(ast);
		json.ShouldNotContain("Quantifier");
		json.ShouldContain("Literal");
	}

	[TestMethod]
	public void BraceQuantifierStillWorks()
	{
		// {3} after a literal should still parse as a quantifier.
		RegexParser p = new("a{3}");
		Ast.RegexNode ast = p.Parse();
		Ast.QuantifierNode quant = ast.ShouldBeOfType<QuantifierNode>();
		quant.Min.ShouldBe(3);
		quant.Max.ShouldBe(3);
	}

	[TestMethod]
	public void BraceRangeQuantifierStillWorks()
	{
		RegexParser p = new("a{2,5}");
		Ast.RegexNode ast = p.Parse();
		Ast.QuantifierNode quant = ast.ShouldBeOfType<QuantifierNode>();
		quant.Min.ShouldBe(2);
		quant.Max.ShouldBe(5);
	}

	[TestMethod]
	public void ParsesOriginalBugReportPattern()
	{
		// Full pattern from the original bug report.
		const string pattern = """(?n)"geometry":{"x":(?<Long>-?\d+(\.\d+)?),"y":(?<Lat>-?\d+(\.\d+)?)(,"z":(?<Alt>-?\d+(\.\d+)?))?}""";
		RegexParser p = new(pattern);
		Ast.RegexNode ast = p.Parse();
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
		Ast.RegexNode ast = p.Parse();
		Ast.GroupNode group = ast.ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeFalse();
	}

	[TestMethod]
	public void ExplicitCaptureOptionPreservesNamedGroupCapturing()
	{
		// Named groups should still capture even with ExplicitCapture.
		RegexParser p = new("(?<name>abc)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		Ast.RegexNode ast = p.Parse();
		Ast.GroupNode group = ast.ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
		group.Name.ShouldBe("name");
	}

	[TestMethod]
	public void InlineOptionNMakesSubsequentUnnamedGroupNonCapturing()
	{
		// (?n) followed by (...) — the unnamed group should become non-capturing.
		RegexParser p = new("(?n)(abc)");
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);
		seq.Items[0].ShouldBeOfType<InlineOptionsNode>().Options.ShouldBe("n");
		Ast.GroupNode group = seq.Items[1].ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeFalse();
	}

	[TestMethod]
	public void InlineOptionNDoesNotAffectNamedGroups()
	{
		// (?n) followed by (?<name>...) — named groups should still capture.
		RegexParser p = new("(?n)(?<name>abc)");
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);
		Ast.GroupNode group = seq.Items[1].ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
		group.Name.ShouldBe("name");
	}

	[TestMethod]
	public void ScopedInlineOptionNIsRestoredAfterGroup()
	{
		// (?n:(a))(b) — inside (?n:...) the unnamed group is non-capturing,
		// but outside it the unnamed group should be capturing again.
		RegexParser p = new("(?n:(a))(b)");
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);

		// The scoped group (?n:...) itself is non-capturing.
		Ast.GroupNode scopedGroup = seq.Items[0].ShouldBeOfType<GroupNode>();
		scopedGroup.IsCapturing.ShouldBeFalse();

		// Inner (a) should be non-capturing because of (?n:...).
		Ast.GroupNode innerGroup = scopedGroup.Inner.ShouldBeOfType<GroupNode>();
		innerGroup.IsCapturing.ShouldBeFalse();

		// Outer (b) should be capturing because n scope has ended.
		Ast.GroupNode outerGroup = seq.Items[1].ShouldBeOfType<GroupNode>();
		outerGroup.IsCapturing.ShouldBeTrue();
	}

	[TestMethod]
	public void InlineOptionMinusNReEnablesCapturing()
	{
		// With ExplicitCapture, (?-n) should re-enable unnamed capturing.
		RegexParser p = new("(?-n)(abc)", System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		Ast.RegexNode ast = p.Parse();
		Ast.SequenceNode seq = ast.ShouldBeOfType<SequenceNode>();
		seq.Items.Count.ShouldBe(2);
		seq.Items[0].ShouldBeOfType<InlineOptionsNode>().Options.ShouldBe("-n");
		Ast.GroupNode group = seq.Items[1].ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
	}

	[TestMethod]
	public void WithoutExplicitCaptureUnnamedGroupIsCapturing()
	{
		// Baseline: without ExplicitCapture, (...) is capturing.
		RegexParser p = new("(abc)");
		Ast.RegexNode ast = p.Parse();
		Ast.GroupNode group = ast.ShouldBeOfType<GroupNode>();
		group.IsCapturing.ShouldBeTrue();
	}

	#endregion
}
