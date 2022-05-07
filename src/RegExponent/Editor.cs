namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Windows;
	using System.Windows.Input;
	using ICSharpCode.AvalonEdit;
	using ICSharpCode.AvalonEdit.Document;
	using ICSharpCode.AvalonEdit.Editing;
	using ICSharpCode.AvalonEdit.Search;

	#endregion

	internal sealed class Editor : TextEditor
	{
		#region Private Data Members

		private static readonly string[] SupportedNewlines = new[] { "\n", "\r\n" };

		private Model? model;

		#endregion

		#region Constructors

		public Editor()
		{
			this.TextArea.TextEntering += this.TextAreaTextEntering;
			DataObject.AddPastingHandler(this, this.OnPaste);
			SearchPanel.Install(this);
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

		#endregion

		#region Private Methods

		private static void SetSelection(TextArea area, string text)
		{
			area.Selection.ReplaceSelectionWithText(text);
			area.Caret.BringCaretToView();
		}

		private void OnPaste(object sender, DataObjectPastingEventArgs e)
		{
			// https://stackoverflow.com/a/11306145/1882616
			bool hasText = e.SourceDataObject.GetDataPresent(typeof(string));
			if (!hasText)
			{
				e.CancelCommand();
			}
			else if (sender is TextEditor editor)
			{
				// TextEditor's default paste will normalize to Environment.NewLine if the editor is empty.
				// We'll override that behavior so we know our Newline is always used.
				string text = e.SourceDataObject.GetData(typeof(string)) as string ?? string.Empty;
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

		#endregion
	}
}
