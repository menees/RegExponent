namespace RegExponent.Highlights;

using ICSharpCode.AvalonEdit.Document;

internal sealed class PatternBracketMatcher : BracketMatcher
{
	private const char Escape = '\\';

	public bool XMode { get; set; }

	protected override int SearchForward(IDocument document, int offset, char openBracket, char closingBracket)
	{
		int result = -1;

		if (this.CanSearch(document, offset))
		{
			result = base.SearchForward(document, offset, openBracket, closingBracket);
		}

		return result;
	}

	protected override int SearchBackward(IDocument document, int offset, char openBracket, char closingBracket)
	{
		int result = -1;

		if (this.CanSearch(document, offset))
		{
			result = base.SearchBackward(document, offset, openBracket, closingBracket);
		}

		return result;
	}

	protected override bool IsEscaped(IDocument document, int offset)
	{
		bool result = offset > 0 && document.GetCharAt(offset - 1) == Escape;
		return result;
	}

	private bool CanSearch(IDocument document, int offset)
	{
		bool result = true;

		// Skip searching if the char at the original offset is escaped or in an x-mode comment.
		if (this.IsEscaped(document, offset))
		{
			result = false;
		}
		else if (this.XMode)
		{
			// Search backward to the beginning of the line for an # comment marker that's not in a character class.
			IDocumentLine line = document.GetLineByOffset(offset);
			int charClassDepth = 0;
			int minOffset = line.Offset;
			for (int index = offset; index >= minOffset; --index)
			{
				char ch = document.GetCharAt(index);
				if (ch == ']' && !this.IsEscaped(document, index))
				{
					charClassDepth++;
				}
				else if (charClassDepth > 0 && ch == '[' && !this.IsEscaped(document, index))
				{
					charClassDepth--;
				}
				else if (charClassDepth == 0 && ch == '#' && !this.IsEscaped(document, index))
				{
					result = false;
					break;
				}
			}
		}

		return result;
	}
}
