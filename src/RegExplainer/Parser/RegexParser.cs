namespace RegExplainer.Parser;

#region Using Directives

using RegExplainer;
using RegExplainer.Ast;

#endregion

// Simplified but feature-rich recursive-descent regex parser for explanations.
public sealed class RegexParser
{
	#region Private Data Members

	private const int DecimalBase = 10;
	private const int HexadecimalBase = 16;

	private readonly System.Text.RegularExpressions.RegexOptions options;
	private readonly string text;
	private bool explicitCapture;
	private bool extendedMode;
	private int pos;

	#endregion

	#region Constructors

	public RegexParser(string text, System.Text.RegularExpressions.RegexOptions options = System.Text.RegularExpressions.RegexOptions.None)
	{
		this.text = text ?? string.Empty;
		this.pos = 0;
		this.options = options;
		this.explicitCapture = options.HasFlag(System.Text.RegularExpressions.RegexOptions.ExplicitCapture);
		this.extendedMode = options.HasFlag(System.Text.RegularExpressions.RegexOptions.IgnorePatternWhitespace);
	}

	#endregion

	#region Private Enums

	private enum DigitCount
	{
		Two = 2,
		Four = 4,
	}

	#endregion

	#region Public Methods

	public RegexNode Parse()
	{
		RegexNode node = this.ParseAlternation();
		if (!this.IsAtEnd())
		{
			throw this.CreateError($"Unexpected character: '{this.Peek()}'");
		}

		return node;
	}

	public bool TryParse(out RegexNode? ast) => this.TryParse(out ast, out _);

	public bool TryParse(out RegexNode? ast, out RegexParseException? error)
	{
		bool result;
		try
		{
			ast = this.Parse();
			error = null;
			result = true;
		}
		catch (RegexParseException ex)
		{
			ast = null;
			error = ex;
			result = false;
		}

		return result;
	}

	#endregion

	#region Private Methods

	private static bool IsMetaChar(char c) =>
		c is '(' or ')' or '[' or ']' or '|' or '\\' or '.' or '^' or '$' or '*' or '+' or '?' or '{';

	private char Advance() => this.text[this.pos++];

	private RegexParseException CreateError(string message)
	{
		// compute line/column
		int line = 1, col = 1;
		for (int i = 0; i < this.pos && i < this.text.Length; i++)
		{
			if (this.text[i] == '\n')
			{
				line++;
				col = 1;
			}
			else
			{
				col++;
			}
		}

		return new RegexParseException(message, this.pos, line, col);
	}

	private void Expect(char c)
	{
		if (this.IsAtEnd() || this.Peek() != c)
		{
			throw this.CreateError($"Expected '{c}'");
		}

		this.Advance();
	}

	private bool IsAtEnd() => this.pos >= this.text.Length;

	private bool MatchString(string s)
	{
		bool result = false;
		if (this.pos + s.Length <= this.text.Length &&
			string.Compare(this.text, this.pos, s, 0, s.Length, StringComparison.Ordinal) == 0)
		{
			this.pos += s.Length;
			result = true;
		}

		return result;
	}

	private RegexNode ParseAlternation()
	{
		int startPos = this.pos;
		List<RegexNode> alternatives = [this.ParseSequence()];
		while (!this.IsAtEnd() && this.Peek() == '|')
		{
			this.Advance();
			alternatives.Add(this.ParseSequence());
		}

		RegexNode result;
		if (alternatives.Count == 1)
		{
			result = alternatives[0];
		}
		else
		{
			AlternationNode alt = new();
			foreach (RegexNode a in alternatives)
			{
				alt.Alternatives.Add(a);
			}

			alt.Start = startPos;
			alt.End = this.pos;
			result = alt;
		}

		return result;
	}

	private RegexNode? ParseAtom()
	{
		RegexNode? result = null;
		if (!this.IsAtEnd())
		{
			char c = this.Peek();
			result = c switch
			{
				'(' => this.ParseGroupOrLookaround(),
				'[' => this.ParseCharacterClass(),
				'\\' => this.ParseEscapeOrBackref(),
				'.' => this.WithSpan(() =>
					{
						this.Advance();
						return new Ast.DotNode();
					}),
				'^' => this.WithSpan(() =>
					{
						this.Advance();
						return new Ast.AnchorNode(Ast.AnchorKind.Start);
					}),
				'$' => this.WithSpan(() =>
					{
						this.Advance();
						return new Ast.AnchorNode(Ast.AnchorKind.End);
					}),
				'{' or '}' => this.WithSpan(() =>
					{
						char ch = this.Advance();
						return new Ast.LiteralNode(ch.ToString());
					}),
				_ => this.ParseLiteralRun(),
			};
		}

		return result;
	}

	private CharacterClassNode ParseCharacterClass()
	{
		return this.WithSpan(() =>
		{
			this.Advance(); // '['
			bool neg = false;
			if (!this.IsAtEnd() && this.Peek() == '^')
			{
				neg = true;
				this.Advance();
			}

			CharacterClassNode cc = new(neg);

			// full POSIX-like bracket grammar simplified: allow [:class:], [=.=], [.] forms
			while (!this.IsAtEnd() && this.Peek() != ']')
			{
				if (this.Peek() == '\\')
				{
					Ast.CharacterClassItem? esc = this.ParseClassEscape();
					if (esc != null)
					{
						cc.Items.Add(esc);
					}

					continue;
				}

				// POSIX character class e.g. [[:digit:]]
				if (this.Peek() == '[' && this.pos + 1 < this.text.Length && this.text[this.pos + 1] == ':')
				{
					this.pos += 2; // skip '[:'
					ReadOnlySpan<char> nameSpan = this.ParseUntilSpan(':');
					this.Expect(':');
					this.Expect(']');
					cc.Items.Add(new Ast.CategoryItem($"[:{new string(nameSpan)}:]"));
					continue;
				}

				if (this.pos + 2 < this.text.Length && this.text[this.pos + 1] == '-' && this.text[this.pos] != '-' && this.text[this.pos + 2] != ']')
				{
					char from = this.Advance();
					this.Advance();
					char to = this.Advance();
					cc.Items.Add(new Ast.RangeItem(from, to));
					continue;
				}

				char ch = this.Advance();
				cc.Items.Add(new Ast.SingleCharItem(ch));
			}

			this.Expect(']');
			return cc;
		});
	}

	private Ast.CharacterClassItem? ParseClassEscape()
	{
		this.Expect('\\');
		if (this.IsAtEnd())
		{
			throw this.CreateError("Trailing backslash in character class");
		}

		Ast.CharacterClassItem? result;
		if (this.Peek() == 'p' || this.Peek() == 'P')
		{
			char p = this.Advance();
			if (this.IsAtEnd() || this.Peek() != '{')
			{
				throw this.CreateError($"\\{p} must be followed by {{Category}}");
			}

			this.Advance();
			ReadOnlySpan<char> nameSpan = this.ParseUntilSpan('}');
			this.Expect('}');
			result = new Ast.CategoryItem($"\\{p}{{{new string(nameSpan)}}}");
		}
		else
		{
			char c = this.Advance();
			result = c switch
			{
				'd' => new Ast.CategoryItem("\\d"),
				'w' => new Ast.CategoryItem("\\w"),
				's' => new Ast.CategoryItem("\\s"),
				'D' => new Ast.CategoryItem("\\D"),
				'W' => new Ast.CategoryItem("\\W"),
				'S' => new Ast.CategoryItem("\\S"),
				'0' => new Ast.SingleCharItem('\0'),
				'n' => new Ast.SingleCharItem('\n'),
				'r' => new Ast.SingleCharItem('\r'),
				't' => new Ast.SingleCharItem('\t'),
				'f' => new Ast.SingleCharItem('\f'),
				'v' => new Ast.SingleCharItem('\v'),
				'a' => new Ast.SingleCharItem('\a'),
				'e' => new Ast.SingleCharItem('\e'),
				'x' => new Ast.SingleCharItem(this.ParseHexEscape(DigitCount.Two)),
				'u' => new Ast.SingleCharItem(this.ParseHexEscape(DigitCount.Four)),
				'c' => new Ast.SingleCharItem(this.ParseControlChar()),
				'\\' => new Ast.SingleCharItem('\\'),
				_ when !char.IsLetterOrDigit(c) => new Ast.SingleCharItem(c),
				_ => throw this.CreateError($"Unrecognized escape sequence in character class: \\{c}"),
			};
		}

		return result;
	}

	private char ParseControlChar()
	{
		if (this.IsAtEnd())
		{
			throw this.CreateError("Expected control character after \\c");
		}

		char c = this.Advance();
		char result;
		if (c is >= 'A' and <= 'Z')
		{
			result = (char)(c - 'A' + 1);
		}
		else if (c is >= 'a' and <= 'z')
		{
			result = (char)(c - 'a' + 1);
		}
		else
		{
			throw this.CreateError($"Invalid control character: \\c{c}");
		}

		return result;
	}

	private RegexNode ParseEscapeOrBackref()
	{
		int startPos = this.pos;
		RegexNode node = this.ParseEscapeOrBackrefCore();
		node.Start = startPos;
		node.End = this.pos;
		return node;
	}

	private RegexNode ParseEscapeOrBackrefCore()
	{
		this.Expect('\\');
		if (this.IsAtEnd())
		{
			throw this.CreateError("Trailing backslash");
		}

		RegexNode result;
		if (this.Peek() == '0')
		{
			this.Advance();
			if (!this.IsAtEnd() && this.Peek() >= '0' && this.Peek() <= '7')
			{
				throw this.CreateError("Octal escape sequences (\\0nn) are not supported; use \\xHH instead");
			}

			result = new Ast.LiteralNode("\0");
		}
		else if (char.IsDigit(this.Peek()))
		{
			int num = this.ParseNumber();
			result = new Ast.BackreferenceNode(num);
		}
		else if (this.Peek() == 'k')
		{
			this.Advance();
			if (!this.IsAtEnd() && this.Peek() == '<')
			{
				this.Advance();
				ReadOnlySpan<char> nameSpan = this.ParseUntilSpan('>');
				this.Expect('>');
				result = new Ast.NamedBackreferenceNode(new string(nameSpan));
			}
			else
			{
				throw this.CreateError("\\k must be followed by <name>");
			}
		}
		else if (this.Peek() == 'p' || this.Peek() == 'P')
		{
			char p = this.Advance();
			if (this.IsAtEnd() || this.Peek() != '{')
			{
				throw this.CreateError($"\\{p} must be followed by {{Category}}");
			}

			this.Advance();
			ReadOnlySpan<char> nameSpan = this.ParseUntilSpan('}');
			this.Expect('}');
			result = new Ast.EscapeNode($"\\{p}{{{new string(nameSpan)}}}");
		}
		else
		{
			char c = this.Advance();
			result = c switch
			{
				'd' => new Ast.EscapeNode("\\d"),
				'w' => new Ast.EscapeNode("\\w"),
				's' => new Ast.EscapeNode("\\s"),
				'D' => new Ast.EscapeNode("\\D"),
				'W' => new Ast.EscapeNode("\\W"),
				'S' => new Ast.EscapeNode("\\S"),
				'b' => new Ast.EscapeNode("\\b"),
				'B' => new Ast.EscapeNode("\\B"),
				'A' => new Ast.EscapeNode("\\A"),
				'z' => new Ast.EscapeNode("\\z"),
				'Z' => new Ast.EscapeNode("\\Z"),
				'G' => new Ast.EscapeNode("\\G"),
				'n' => new Ast.LiteralNode("\n"),
				'r' => new Ast.LiteralNode("\r"),
				't' => new Ast.LiteralNode("\t"),
				'f' => new Ast.LiteralNode("\f"),
				'v' => new Ast.LiteralNode("\v"),
				'a' => new Ast.LiteralNode("\a"),
				'e' => new Ast.LiteralNode("\e"),
				'x' => new Ast.LiteralNode(this.ParseHexEscape(DigitCount.Two).ToString()),
				'u' => new Ast.LiteralNode(this.ParseHexEscape(DigitCount.Four).ToString()),
				'c' => new Ast.LiteralNode(this.ParseControlChar().ToString()),
				_ when !char.IsLetterOrDigit(c) => new Ast.LiteralNode(c.ToString()),
				_ => throw this.CreateError($"Unrecognized escape sequence: \\{c}"),
			};
		}

		return result;
	}

	private RegexNode ParseGroupOrLookaround()
	{
		// caller positioned at '('
		this.Advance(); // consume '('
		if (this.IsAtEnd())
		{
			throw this.CreateError("Unterminated group");
		}

		// record the position of the opening '(' so we can set proper span values
		int startPos = this.pos - 1;

		RegexNode result;

		// simple capturing group: (...)
		// With ExplicitCapture / (?n), unnamed groups become non-capturing.
		if (this.Peek() != '?')
		{
			RegexNode inner = this.ParseAlternation();
			this.Expect(')');
			result = new GroupNode(inner, isCapturing: !this.explicitCapture)
			{
				Start = startPos,
				End = this.pos,
			};
		}
		else
		{
			// now at '(?'
			this.Advance(); // consume '?'
			if (this.IsAtEnd())
			{
				throw this.CreateError("Unterminated group after '(?'");
			}

			result = this.ParseExtendedGroup(startPos);
		}

		return result;
	}

	private RegexNode ParseExtendedGroup(int startPos)
	{
		RegexNode result;

		// non-capturing (?:...)
		if (this.Peek() == ':')
		{
			this.Advance();
			RegexNode inner = this.ParseAlternation();
			this.Expect(')');
			result = new GroupNode(inner, isCapturing: false)
			{
				Start = startPos,
				End = this.pos,
			};
		}
		else if (this.Peek() == '(')
		{
			// conditional (?(cond)yes|no)
			this.Advance();
			ReadOnlySpan<char> conditionSpan = this.ParseUntilSpan(')');
			this.Expect(')');
			string condition = new(conditionSpan);
			RegexNode trueBranch = this.ParseSequence();
			RegexNode? falseBranch = null;
			if (!this.IsAtEnd() && this.Peek() == '|')
			{
				this.Advance();
				falseBranch = this.ParseSequence();
			}

			this.Expect(')');
			result = new ConditionalNode(condition, trueBranch, falseBranch)
			{
				Start = startPos,
				End = this.pos,
			};
		}
		else if (this.MatchString("<="))
		{
			// lookaround / lookbehind: (?<= ... ), (?<! ... ), (?= ... ), (?! ... )
			result = this.ParseLookaroundGroup(startPos, LookaroundKind.PositiveLookbehind);
		}
		else if (this.MatchString("<!"))
		{
			result = this.ParseLookaroundGroup(startPos, LookaroundKind.NegativeLookbehind);
		}
		else if (this.MatchString("="))
		{
			result = this.ParseLookaroundGroup(startPos, LookaroundKind.PositiveLookahead);
		}
		else if (this.MatchString("!"))
		{
			result = this.ParseLookaroundGroup(startPos, LookaroundKind.NegativeLookahead);
		}
		else if (this.MatchString("P<") || this.MatchString("<"))
		{
			// named capture (?<name>...) or (?P<name>...)
			ReadOnlySpan<char> nameSpan = this.ParseUntilSpan('>');
			this.Expect('>');
			string name = new(nameSpan);
			RegexNode inner = this.ParseAlternation();
			this.Expect(')');
			result = new GroupNode(inner, isCapturing: true, name: name)
			{
				Start = startPos,
				End = this.pos,
			};
		}
		else
		{
			result = this.ParseInlineOptionsGroup(startPos);
		}

		return result;
	}

	private RegexNode ParseInlineOptionsGroup(int startPos)
	{
		// inline options (?i:...), (?n), etc.
		int savePos = this.pos;
		while (!this.IsAtEnd() && this.Peek() != ':')
		{
			if (this.Peek() == ')' || this.Peek() == '<' || this.Peek() == '=')
			{
				break;
			}

			this.Advance();
		}

		RegexNode result;
		if (!this.IsAtEnd() && this.Peek() == ':')
		{
			string optsStr = new(this.text.AsSpan(savePos, this.pos - savePos));
			this.Advance();
			bool savedExtended = this.extendedMode;
			bool savedExplicit = this.explicitCapture;
			this.ApplyInlineOptions(optsStr);
			RegexNode inner = this.ParseAlternation();
			this.extendedMode = savedExtended;
			this.explicitCapture = savedExplicit;
			this.Expect(')');
			result = new Ast.GroupNode(inner, isCapturing: false, name: null, inlineOptions: optsStr)
			{
				Start = startPos,
				End = this.pos,
			};
		}
		else if (!this.IsAtEnd() && this.Peek() == ')' && this.pos > savePos)
		{
			// Standalone inline options like (?n), (?i), (?i-s), etc.
			string optsStr = new(this.text.AsSpan(savePos, this.pos - savePos));
			this.Advance(); // consume ')'
			this.ApplyInlineOptions(optsStr);
			result = new Ast.InlineOptionsNode(optsStr)
			{
				Start = startPos,
				End = this.pos,
			};
		}
		else
		{
			this.pos = savePos;
			throw this.CreateError("Unknown group construct");
		}

		return result;
	}

	private char ParseHexEscape(DigitCount digitCount)
	{
		int count = (int)digitCount;
		int value = 0;
		for (int i = 0; i < count; i++)
		{
			if (this.IsAtEnd())
			{
				throw this.CreateError($"Expected {count} hex digits in escape sequence, got {i}");
			}

			char h = this.Peek();
			if (h is not ((>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F')))
			{
				throw this.CreateError($"Expected hex digit, got '{h}'");
			}

			this.Advance();
			value = (value * HexadecimalBase) + h switch
			{
				>= '0' and <= '9' => h - '0',
				>= 'a' and <= 'f' => h - 'a' + DecimalBase,
				_ => h - 'A' + DecimalBase,
			};
		}

		return (char)value;
	}

	private LiteralNode ParseLiteralRun()
	{
		return this.WithSpan(() =>
		{
			int start = this.pos;
			while (!this.IsAtEnd())
			{
				char c = this.Peek();
				if (IsMetaChar(c))
				{
					break;
				}

				if (this.extendedMode && (char.IsWhiteSpace(c) || c == '#'))
				{
					break;
				}

				this.Advance();
			}

			int len = this.pos - start;
			if (len == 0)
			{
				throw this.CreateError("Unexpected char");
			}

			return new Ast.LiteralNode(new string(this.text.AsSpan(start, len)));
		});
	}

	private LookaroundNode ParseLookaroundGroup(int startPos, LookaroundKind kind)
	{
		RegexNode inner = this.ParseAlternation();
		this.Expect(')');
		return new Ast.LookaroundNode(inner, kind)
		{
			Start = startPos,
			End = this.pos,
		};
	}

	private int ParseNumber()
	{
		if (this.IsAtEnd() || !char.IsDigit(this.Peek()))
		{
			throw this.CreateError("Number expected");
		}

		int v = 0;
		while (!this.IsAtEnd() && char.IsDigit(this.Peek()))
		{
			v = (v * DecimalBase) + (this.Advance() - '0');
		}

		return v;
	}

	private (int Min, int? Max, bool IsLazy) ParseQuantifierDetails(char quantChar)
	{
		int min;
		int? max;

		switch (quantChar)
		{
			case '*': min = 0; max = null; this.Advance(); break;
			case '+': min = 1; max = null; this.Advance(); break;
			case '?': min = 0; max = 1; this.Advance(); break;
			case '{':
				this.Advance();
				min = this.ParseNumber();
				if (this.IsAtEnd())
				{
					throw this.CreateError("Unterminated quantifier");
				}

				if (this.Peek() == ',')
				{
					this.Advance();
					if (this.IsAtEnd())
					{
						throw this.CreateError("Unterminated quantifier");
					}

					max = this.Peek() == '}' ? null : this.ParseNumber();
				}
				else
				{
					max = min;
				}

				this.Expect('}');
				break;
			default:
				throw this.CreateError($"Unexpected quantifier character: '{quantChar}'");
		}

		bool isLazy = !this.IsAtEnd() && this.Peek() == '?';
		if (isLazy)
		{
			this.Advance();
		}

		return (min, max, isLazy);
	}

	private RegexNode ParseQuantifierIfAny(RegexNode atom)
	{
		RegexNode result = atom;
		if (!this.IsAtEnd())
		{
			char c = this.Peek();
			if (c == '*' || c == '+' || c == '?' || (c == '{' && this.IsQuantifierBrace()))
			{
				// If the atom is a multi-character literal, a following quantifier applies only
				// to the last character. Split the literal into a prefix (if any) and the last
				// character, then attach the quantifier to the last character node.
				if (atom is LiteralNode lit && lit.Text.Length > 1)
				{
					int? litStart = lit.Start;
					string text = lit.Text;
					int n = text.Length;
					string prefix = text[..(n - 1)];
					string last = text.Substring(n - 1, 1);

					LiteralNode? prefixNode = null;
					if (prefix.Length > 0)
					{
						prefixNode = new Ast.LiteralNode(prefix)
						{
							Start = litStart,
							End = litStart + prefix.Length,
						};
					}

					LiteralNode lastNode = new(last)
					{
						Start = litStart + (prefix?.Length ?? 0),
					};
					lastNode.End = lastNode.Start + 1;

					(int min, int? max, bool isLazy) = this.ParseQuantifierDetails(c);

					QuantifierNode quant = new(lastNode, min, max, isLazy)
					{
						// span: start at last char, end at current parser position
						Start = lastNode.Start,
						End = this.pos,
					};

					if (prefixNode == null)
					{
						result = quant;
					}
					else
					{
						SequenceNode seq = new();
						seq.Items.Add(prefixNode);
						seq.Items.Add(quant);
						seq.Start = prefixNode.Start;
						seq.End = quant.End;
						result = seq;
					}
				}
				else
				{
					// Non-literal atoms: span covers the target and the quantifier symbol
					(int min, int? max, bool isLazy) = this.ParseQuantifierDetails(c);
					result = new Ast.QuantifierNode(atom, min, max, isLazy)
					{
						Start = atom.Start,
						End = this.pos,
					};
				}
			}
		}

		return result;
	}

	private RegexNode ParseSequence()
	{
		return this.WithSpan(() =>
		{
			SequenceNode seq = new();
			while (!this.IsAtEnd())
			{
				this.SkipExtendedWhitespace();
				if (this.IsAtEnd())
				{
					break;
				}

				char c = this.Peek();
				if (c == '|' || c == ')')
				{
					break;
				}

				if (this.extendedMode && c == '#')
				{
					seq.Items.Add(this.ParseComment());
					continue;
				}

				RegexNode? atom = this.ParseAtom();
				if (atom == null)
				{
					break;
				}

				this.SkipExtendedWhitespace();
				RegexNode q = this.ParseQuantifierIfAny(atom);
				seq.Items.Add(q);
			}

			return seq.Items.Count == 1 ? seq.Items[0] : seq;
		});
	}

	// Return a span from the current position up to (but not including) the terminator.
	// Does not consume the terminator; caller is expected to call Expect(terminator) if needed.
	private ReadOnlySpan<char> ParseUntilSpan(char terminator)
	{
		int start = this.pos;
		while (!this.IsAtEnd() && this.Peek() != terminator)
		{
			this.Advance();
		}

		if (this.IsAtEnd())
		{
			throw this.CreateError($"Terminator '{terminator}' not found");
		}

		return this.text.AsSpan(start, this.pos - start);
	}

	// helpers
	private void ApplyInlineOptions(string options)
	{
		bool afterMinus = false;
		foreach (char c in options)
		{
			if (c == '-')
			{
				afterMinus = true;
			}
			else if (c == 'n')
			{
				this.explicitCapture = !afterMinus;
			}
			else if (c == 'x')
			{
				this.extendedMode = !afterMinus;
			}
		}
	}

	private bool IsQuantifierBrace() => this.pos + 1 < this.text.Length && char.IsDigit(this.text[this.pos + 1]);

	private CommentNode ParseComment()
	{
		return this.WithSpan(() =>
		{
			this.Advance(); // consume '#'
			int start = this.pos;
			while (!this.IsAtEnd() && this.Peek() != '\n')
			{
				this.Advance();
			}

			string text = this.text[start..this.pos].TrimEnd('\r');
			if (!this.IsAtEnd() && this.Peek() == '\n')
			{
				this.Advance();
			}

			return new CommentNode(text);
		});
	}

	private char Peek() => this.text[this.pos];

	private void SkipExtendedWhitespace()
	{
		if (this.extendedMode)
		{
			while (!this.IsAtEnd() && char.IsWhiteSpace(this.Peek()))
			{
				this.Advance();
			}
		}
	}

	private T WithSpan<T>(Func<T> ctor)
		where T : RegexNode
	{
		int start = this.pos;
		T node = ctor();
		node.Start = start;
		node.End = this.pos;
		return node;
	}

	#endregion
}
