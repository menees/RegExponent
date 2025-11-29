namespace RegExponent;

#region Using Directives

using System;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Search;
using RegExponent.Highlights;

#endregion

internal sealed class Editor : TextEditor
{
	#region Private Data Members

	private static readonly string[] SupportedNewlines = ["\n", "\r\n"];

	private Model? model;
	private BracketMatcher? bracketMatcher;
	private BracketHighlighter? bracketHighlighter;

	#endregion

	#region Constructors

	public Editor()
	{
		this.TextArea.TextEntering += this.TextAreaTextEntering;
		DataObject.AddPastingHandler(this, this.OnPaste);
		SearchPanel.Install(this);
		this.PreviewKeyDown += this.EditorPreviewKeyDown;
	}

	#endregion

	#region Private Properties

	private Model Model
	{
		get
		{
			this.model ??= (Model)this.FindResource(nameof(RegExponent.Model));
			return this.model;
		}
	}

	private string Newline => this.Model.Newline;

	#endregion

	#region Public Methods

	public string GetText()
	{
		string result = this.Text;
		if (result.Contains('\n'))
		{
			// Since we monitor Paste and Enter input, the control text will usually
			// contain correctly normalized text. But if the text was loaded from a
			// file where the newline property or text values were fiddled with, then
			// we might need to fix the newlines here.
			//
			// We don't need to push the normalized value back into this.Text here
			// because typically the MainWindow will call SetText next, and it checks
			// for differences.
			if ((this.Newline.StartsWith('\r') && !result.Contains('\r'))
				|| (!this.Newline.StartsWith('\r') && result.Contains('\r')))
			{
				result = this.NormalizeNewlines(result);
			}
		}

		return result;
	}

	public void SetText(string text)
	{
		string currentText = this.Text;
		if (currentText != text)
		{
			// Try to restore line and column position (instead of selection start and length)
			// so the caret stays in the same visible position when newlines are converted.
			// Also, if a file is loading and completely changing the text, then preserving the
			// current selection isn't meaningful.
			Caret caret = this.TextArea.Caret;
			int caretLine = caret.Line;
			int caretColumn = caret.Column;
			bool caretAtEnd = this.CaretOffset == this.Document.TextLength;

			this.Text = text;
			if (caretAtEnd)
			{
				this.CaretOffset = this.Document.TextLength;
			}
			else
			{
				caret = this.TextArea.Caret;
				caret.Line = caretLine;
				caret.Column = caretColumn;
			}

			caret.BringCaretToView();
		}
	}

	public void HighlightBrackets()
	{
		BracketMatch? match = this.bracketMatcher?.MatchBracket(this.Document, this.TextArea.Caret.Offset);
		this.bracketHighlighter?.SetMatch(match);
	}

	public void SetBracketMatcher(BracketMatcher? matcher)
	{
		if (this.bracketMatcher != matcher)
		{
			this.bracketMatcher = matcher;

			if (this.bracketMatcher != null)
			{
				this.bracketHighlighter = new(this.TextArea.TextView);
			}
			else
			{
				this.bracketHighlighter = null;
			}
		}
	}

	#endregion

	#region Private Methods

	private static void SetSelection(TextArea area, string text)
	{
		area.Selection.ReplaceSelectionWithText(text);
		area.Caret.BringCaretToView();
	}

	private void OnPaste(object sender, DataObjectPastingEventArgs e)
	{
		// It's better to explicitly request DataFormats.UnicodeText here than typeof(string).
		// Using typeof(string) seems to only use the current Windows code page, so higher
		// characters get lossy converted to '?'. With DataFormats.UnicodeText no loss occurs.
		// https://stackoverflow.com/a/7617087/1882616
		bool hasText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
		if (!hasText)
		{
			e.CancelCommand();
		}
		else if (sender is TextEditor editor)
		{
			// TextEditor's default paste will normalize to Environment.NewLine if the editor is empty.
			// We'll override that behavior so we know our Newline is always used.
			string text = e.SourceDataObject.GetData(DataFormats.UnicodeText, true) as string ?? string.Empty;
			text = this.NormalizeNewlines(text);

			TextArea area = editor.TextArea;
			SetSelection(area, text);
			e.CancelCommand();
		}
	}

	private string NormalizeNewlines(string text)
	{
		string[] lines = text.Split(SupportedNewlines, StringSplitOptions.None);
		text = string.Join(this.Newline, lines);
		return text;
	}

	private void TextAreaTextEntering(object sender, TextCompositionEventArgs e)
	{
		if (e.Text == "\n" && sender is TextArea area)
		{
			SetSelection(area, this.Newline);
			e.Handled = true;
		}
	}

	private void EditorPreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Tab)
		{
			// https://stackoverflow.com/a/40677396/1882616
			ModifierKeys modifiers = Keyboard.Modifiers;
			FocusNavigationDirection? direction = null;

			if ((modifiers == ModifierKeys.Shift && this.IsReadOnly) || (modifiers == (ModifierKeys.Control | ModifierKeys.Shift)))
			{
				direction = FocusNavigationDirection.Previous;
			}
			else if ((modifiers == ModifierKeys.None && this.IsReadOnly) || (modifiers == ModifierKeys.Control))
			{
				direction = FocusNavigationDirection.Next;
			}

			if (direction != null)
			{
				// https://github.com/icsharpcode/AvalonEdit/issues/118#issuecomment-447380654
				// https://social.msdn.microsoft.com/Forums/vstudio/en-US/007a0905-a05b-4009-930c-e206804b6a39/programatically-set-focus-on-the-next-control?forum=wpf
				TraversalRequest traversal = new(direction.Value);
				if (direction == FocusNavigationDirection.Previous)
				{
					this.MoveFocus(traversal);
				}
				else
				{
					this.TextArea.MoveFocus(traversal);
				}

				e.Handled = true;
			}
		}
	}

	#endregion
}
