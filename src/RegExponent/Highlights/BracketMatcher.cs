namespace RegExponent.Highlights;

#region Using Directives

using ICSharpCode.AvalonEdit.Document;

#endregion

internal class BracketMatcher
{
	#region Private Data Members

	private readonly string openingBrackets;
	private readonly string closingBrackets;

	#endregion

	#region Constructors

	public BracketMatcher(string openingBrackets = "([{<", string closingBrackets = ")]}>")
	{
		this.openingBrackets = openingBrackets;
		this.closingBrackets = closingBrackets;
	}

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

			int index = this.closingBrackets.IndexOf(ch);
			if (index >= 0)
			{
				int openingOffset = this.SearchBackward(document, offset, this.openingBrackets[index], this.closingBrackets[index]);
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
				index = this.openingBrackets.IndexOf(ch);
				if (index >= 0)
				{
					int closingOffset = this.SearchForward(document, offset, this.openingBrackets[index], this.closingBrackets[index]);
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
		offset++;

		int length = document.TextLength;
		for (int index = offset; index < length; index++)
		{
			char ch = document.GetCharAt(index);
			if (ch == openBracket && !this.IsEscaped(document, index))
			{
				openBracketDepth++;
			}
			else if (ch == closingBracket && !this.IsEscaped(document, index))
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
		offset--;

		for (int index = offset; index >= 0; index--)
		{
			char ch = document.GetCharAt(index);
			if (ch == openBracket && !this.IsEscaped(document, index))
			{
				closingBracketDepth--;
				if (closingBracketDepth == 0)
				{
					result = index;
					break;
				}
			}
			else if (ch == closingBracket && !this.IsEscaped(document, index))
			{
				closingBracketDepth++;
			}
		}

		return result;
	}

	protected virtual bool IsEscaped(IDocument document, int offset) => false;

	#endregion
}
