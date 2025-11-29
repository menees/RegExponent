namespace RegExponent.Highlights;

#region Using Directives

using ICSharpCode.AvalonEdit.Document;

#endregion

internal class BracketMatcher
{
	#region Private Data Members

	private static readonly string OpeningBrackets = "([{<";
	private static readonly string ClosingBrackets = ")]}>";

	#endregion

	#region Public Methods

	public BracketMatch? MatchBracket(IDocument document, int offset)
	{
		BracketMatch? result = null;

		// AvalonEdit offsets are between characters, and the passed-in offset is always after the
		// "most recently typed" character (unless the caret is hopping around).
		offset--;

		if (offset >= 0)
		{
			char ch = document.GetCharAt(offset);

			int index = ClosingBrackets.IndexOf(ch);
			if (index >= 0)
			{
				int openingOffset = this.SearchBackward(document, offset - 1, OpeningBrackets[index], ClosingBrackets[index]);
				if (openingOffset >= 0)
				{
					result = new()
					{
						OpeningOffset = openingOffset,
						ClosingOffset = offset,
					};
				}
			}
			else
			{
				index = OpeningBrackets.IndexOf(ch);
				if (index >= 0)
				{
					int closingOffset = this.SearchForward(document, offset + 1, OpeningBrackets[index], ClosingBrackets[index]);
					if (closingOffset >= 0)
					{
						result = new()
						{
							OpeningOffset = offset,
							ClosingOffset = closingOffset,
						};
					}
				}
			}
		}

		return result;
	}

	#endregion

	#region Protected Methods

	protected virtual int SearchForward(IDocument document, int offset, char openBracket, char closingBracket)
	{
		int result = -1;
		int openBracketDepth = 1;

		// LONG-TERM-TODO: Make this aware of the regex grammar, e.g. [...], escapes, #-comments, etc. [Bill, 11/28/2025]
		int length = document.TextLength;
		for (int index = offset; index < length; index++)
		{
			char ch = document.GetCharAt(index);
			if (ch == openBracket)
			{
				openBracketDepth++;
			}
			else if (ch == closingBracket)
			{
				openBracketDepth--;
				if (openBracketDepth == 0)
				{
					result = index;
					break;
				}
			}
		}

		return result;
	}

	protected virtual int SearchBackward(IDocument document, int offset, char openBracket, char closingBracket)
	{
		int result = -1;
		int closingBracketDepth = 1;

		// LONG-TERM-TODO: Make this aware of the regex grammar, e.g. [...], escapes, #-comments, etc. [Bill, 11/28/2025]
		for (int index = offset; index >= 0; --index)
		{
			char ch = document.GetCharAt(index);
			if (ch == openBracket)
			{
				closingBracketDepth--;
				if (closingBracketDepth == 0)
				{
					result = index;
					break;
				}
			}
			else if (ch == closingBracket)
			{
				closingBracketDepth++;
			}
		}

		return result;
	}

	#endregion
}
