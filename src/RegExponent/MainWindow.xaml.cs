namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Windows.Threading;
	using Menees;
	using Menees.Windows.Presentation;
	using Microsoft.Win32;
	using Drawing = System.Drawing;
	using WinForms = System.Windows.Forms;

	#endregion

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Private Data Members

		private const string FileDialogDefaultExt = ".rgxp";
		private const string FileDialogFilter = nameof(RegExponent) + " Files (*" + FileDialogDefaultExt + ")|*" + FileDialogDefaultExt;

		private readonly Model model;
		private readonly WindowSaver saver;
		private readonly HashSet<RichTextBox> dirtyText = new();
		private string currentFileName;

		private int updateLevel;
		private UpdateState updateState;

		#endregion

		#region Constructors

		public MainWindow()
		{
			// TODO: Add good icon. [Bill, 4/9/2022]
			this.InitializeComponent();

			this.model = (Model)this.FindResource(nameof(Model));
			this.model.PropertyChanged += this.ModelPropertyChanged;

			// Make sure this.Title and other fileName-related properties initialize properly.
			this.currentFileName = null!;
			this.CurrentFileName = string.Empty;

			this.saver = new WindowSaver(this);
			this.saver.LoadSettings += this.FormSaverLoadSettings;
			this.saver.SaveSettings += this.FormSaverSaveSettings;

			// Only paste plain text and not rich text.
			DataObject.AddPastingHandler(this.pattern, OnPaste);
			DataObject.AddPastingHandler(this.replacement, OnPaste);
			DataObject.AddPastingHandler(this.input, OnPaste);

			// TODO: Handle SystemEvents.SessionEnding. If IsModified, then auto-save and close. [Bill, 3/31/2022]
			// TODO: Support reading a file name from the command line. [Bill, 4/9/2022]
		}

		#endregion

		#region Private Enums

		private enum UpdateState
		{
			None,

			// TODO: Can these states be combined? Do we just need bool isUpdating? [Bill, 4/17/2022]
			ShowingResults,
			Resetting,
		}

		#endregion

		#region Private Properties

		private string CurrentFileName
		{
			get => this.currentFileName;
			set
			{
				if (this.currentFileName != value)
				{
					this.currentFileName = value;
					this.UpdateTitle();
					if (this.currentFileName.IsNotEmpty())
					{
						// TODO: Update recent files. [Bill, 4/9/2022]
					}
				}
			}
		}

		private string ModelDisplayName =>
			this.CurrentFileName.IsNotEmpty()
			? System.IO.Path.GetFileNameWithoutExtension(this.CurrentFileName)
			: "<Untitled>";

		#endregion

		#region Private Methods

		private static void ApplyFont(Control control, Drawing.Font font)
		{
			// From https://stackoverflow.com/a/37578593/1882616
			control.FontFamily = new FontFamily(font.Name);
			switch (font.Unit)
			{
				case Drawing.GraphicsUnit.Point:
					const double XamlPixelsPerInch = 96.0;
					const double DrawingPointsPerInch = 72.0;
					control.FontSize = font.Size * XamlPixelsPerInch / DrawingPointsPerInch;
					break;

				case Drawing.GraphicsUnit.Pixel:
					control.FontSize = font.Size;
					break;
			}

			control.FontWeight = font.Bold ? FontWeights.Bold : FontWeights.Regular;
			control.FontStyle = font.Italic ? FontStyles.Italic : FontStyles.Normal;
		}

		private static void InsertText(RichTextBox richTextBox, string text)
		{
			if (!richTextBox.Selection.IsEmpty)
			{
				richTextBox.Selection.Text = text;
			}
			else
			{
				// Make sure the caret moves to the end of the text after insertion by ensuring LogicalDirection is Forward.
				// We can't directly set TextPointer.LogicalDirection, so we have to replace the TextPointer with a new instance.
				// https://stackoverflow.com/a/2916699/1882616
				richTextBox.CaretPosition = richTextBox.CaretPosition.GetPositionAtOffset(0, LogicalDirection.Forward);
				richTextBox.CaretPosition.InsertTextInRun(text);
			}
		}

		private static void OnPaste(object sender, DataObjectPastingEventArgs e)
		{
			// If rich, formatted text is on the clipboard, we only want to paste it as plain text.
			// https://stackoverflow.com/a/3061506/1882616
			bool hasText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
			if (!hasText)
			{
				e.CancelCommand();
			}
			else
			{
				// Convert any rich text to plain text.
				// https://stackoverflow.com/a/11306145/1882616
				string text = e.SourceDataObject.GetData(DataFormats.UnicodeText, true) as string ?? string.Empty;
				DataObject d = new();
				d.SetData(DataFormats.UnicodeText, text);
				e.DataObject = d;
			}
		}

		private void SetText(RichTextBox richTextBox, string text)
		{
			// TODO: Do intelligent diffs rather than rebuild whole document each time. [Bill, 4/18/2022]
			string[] lines = text.Split(this.model.Newline);
			FlowDocument document = new();
			foreach (string line in lines)
			{
				document.Blocks.Add(new Paragraph(new Run(line)));
			}

			TextSelection selection = richTextBox.Selection;
			int selectionStartOffset = richTextBox.Document.ContentStart.GetOffsetToPosition(selection.Start);
			int selectionLength = selection.Start.GetOffsetToPosition(selection.End);
			LogicalDirection direction = selection.End.LogicalDirection;

			// TODO: Take a lamdba to highlight runs based on syntax or matches. [Bill, 4/15/2022]
			richTextBox.Document = document;

			TextPointer? selectionStart = richTextBox.Document.ContentStart.GetPositionAtOffset(selectionStartOffset, LogicalDirection.Forward);
			TextPointer? selectionEnd = selectionStart?.GetPositionAtOffset(selectionLength, direction);
			if (selectionStart != null && selectionEnd != null)
			{
				richTextBox.Selection.Select(selectionStart, selectionEnd);
			}
		}

		private bool CanClear()
		{
			bool allowClear = !this.model.IsModified;

			if (!allowClear)
			{
				MessageBoxResult wantToSave = MessageBox.Show(
					this,
					$"Do you want to save the changes to {this.ModelDisplayName}?",
					"Save Changes",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question);

				switch (wantToSave)
				{
					case MessageBoxResult.Yes:
						allowClear = this.Save();
						break;

					case MessageBoxResult.No:
						allowClear = true;
						break;
				}
			}

			return allowClear;
		}

		private IDisposable SetState(UpdateState state)
		{
			UpdateState previous = this.updateState;
			this.updateState = state;
			return new Disposer(() => this.updateState = previous);
		}

		private void Clear()
		{
			if (this.CanClear())
			{
				this.CurrentFileName = string.Empty;
				using (this.SetState(UpdateState.Resetting))
				{
					this.model.Clear();
				}

				this.TryQueueUpdate();
			}
		}

		private bool Load(string fileName, bool checkCanClear = true)
		{
			bool result = false;

			if (File.Exists(fileName) && (!checkCanClear || this.CanClear()))
			{
				this.CurrentFileName = FileUtility.ExpandFileName(fileName);
				using (this.SetState(UpdateState.Resetting))
				{
					this.model.Load(fileName);
				}

				this.TryQueueUpdate();
				result = true;
			}

			return result;
		}

		private bool Save(bool forceSaveDialog = false)
		{
			bool trySave = true;

			if (forceSaveDialog || this.CurrentFileName.IsEmpty())
			{
				SaveFileDialog dialog = new();
				dialog.FileName = this.CurrentFileName;
				dialog.DefaultExt = FileDialogDefaultExt;
				dialog.Filter = FileDialogFilter;
				trySave = dialog.ShowDialog(this) ?? false;
				if (trySave)
				{
					this.CurrentFileName = dialog.FileName;
				}
			}

			bool saved = false;
			if (trySave && this.CurrentFileName.IsNotEmpty())
			{
				this.model.Save(this.CurrentFileName);
				saved = true;
			}

			return saved;
		}

		private void UpdateTitle()
		{
			StringBuilder sb = new(nameof(RegExponent));
			sb.Append(" - ");
			sb.Append(this.ModelDisplayName);
			if (this.model.IsModified)
			{
				sb.Append(" - ");
				sb.Append("Modified");
			}

			this.Title = sb.ToString();
		}

		private string GetText(RichTextBox richTextBox)
		{
			StringBuilder sb = new();
			foreach (Block block in richTextBox.Document.Blocks)
			{
				if (sb.Length > 0)
				{
					sb.Append(this.model.Newline);
				}

				TextRange range = new(block.ContentStart, block.ContentEnd);
				string text = range.Text;
				sb.Append(text);
			}

			string result = sb.ToString();
			return result;
		}

		private void TryQueueUpdate()
		{
			if (this.updateState == UpdateState.None)
			{
				this.Dispatcher.BeginInvoke(new Action(this.BeginForegroundUpdate), DispatcherPriority.ApplicationIdle);
			}
		}

		private void BeginForegroundUpdate()
		{
			// Note: We can't call bool Remove(...) first because we need the control to
			// remain in the dirty set while the model's PropertyChanged event is handled.
			if (this.dirtyText.Contains(this.pattern))
			{
				this.model.Pattern = this.GetText(this.pattern);
				this.dirtyText.Remove(this.pattern);
			}

			if (this.dirtyText.Contains(this.replacement))
			{
				this.model.Replacement = this.GetText(this.replacement);
				this.dirtyText.Remove(this.replacement);
			}

			if (this.dirtyText.Contains(this.input))
			{
				this.model.Input = this.GetText(this.input);
				this.dirtyText.Remove(this.input);
			}

			int currentUpdateLevel = Interlocked.Increment(ref this.updateLevel);
			const int TimeoutSeconds = 5;
			Evaluator evaluator = new(this.model, TimeSpan.FromSeconds(TimeoutSeconds), currentUpdateLevel);

			// In .NET Core, we need to use a Task to run in the background. Otherwise, the async-await
			// viral nature will muddy up every method in this class.
			// https://devblogs.microsoft.com/dotnet/migrating-delegate-begininvoke-calls-for-net-core/
			Task.Run(() =>
			{
				evaluator.Evaluate(() => this.updateLevel);
				this.Dispatcher.BeginInvoke(new Action<Evaluator>(this.EndForegroundUpdate), DispatcherPriority.ApplicationIdle, evaluator);
			});
		}

		private void EndForegroundUpdate(Evaluator evaluator)
		{
			if (evaluator.UpdateLevel == this.updateLevel)
			{
				using (this.SetState(UpdateState.ShowingResults))
				{
					this.SetText(this.pattern, this.model.Pattern); // TODO: Highlight regex syntax. [Bill, 4/15/2022]
					this.SetText(this.input, this.model.Input); // TODO: Alternate highlight matches and underline groups. [Bill, 4/15/2022]

					this.timing.Content = $"{evaluator.Elapsed.TotalMilliseconds} ms";
					this.message.Content = evaluator.Exception?.Message;

					var matches = evaluator.Matches
						.SelectMany((match, matchNum) => match.Groups.Cast<Group>()
							.Select((group, groupNum) => (matchNum, groupNum, group))
							.Where(tuple => tuple.group.Success))
						.Select(pair => new
						{
							Match = pair.groupNum == 0 ? pair.matchNum : (int?)null,
							Group = pair.groupNum != 0 ? pair.group.Name : null,
							pair.group.Index,
							pair.group.Length,
							pair.group.Value,
						})
						.ToList();
					this.groupColumn.Visibility = matches.Any(m => m.Group.IsNotEmpty()) ? Visibility.Visible : Visibility.Collapsed;
					this.matchGrid.ItemsSource = matches;

					if (this.model.InReplaceMode)
					{
						this.SetText(this.replacement, this.model.Replacement); // TODO: Highlight ${} replacers. [Bill, 4/15/2022]
						this.SetText(this.replaced, evaluator.Replaced);
					}
					else if (this.model.InSplitMode)
					{
						var splits = evaluator.Splits
							.Select((line, index) => new
							{
								Index = index,
								Value = line,
								line?.Length,
								Comment = line == null ? "null"
									: line.Length == 0 ? "empty"
									: line.IsWhiteSpace() ? "whitespace"
									: double.TryParse(line, out _) || decimal.TryParse(line, out _) ? "numeric"
									: string.Empty,
							});
						this.splitCommentColumn.Visibility = splits.Any(s => s.Comment.IsNotEmpty()) ? Visibility.Visible : Visibility.Collapsed;
						this.splitGrid.ItemsSource = splits;
					}
				}
			}
		}

		#endregion

		#region Private Event Handlers

		private void ExitExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void ShellExecuteExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string text && Uri.TryCreate(text, UriKind.Absolute, out Uri? uri))
			{
				WindowsUtility.ShellExecute(this, uri.ToString());
				e.Handled = true;
			}
		}

		private void HelpExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			const string Help = "https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference";
			WindowsUtility.ShellExecute(this, Help);
			e.Handled = true;
		}

		private void AboutExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			WindowsUtility.ShowAboutBox(this, this.GetType().Assembly, nameof(RegExponent));
		}

		private void FormSaverLoadSettings(object? sender, SettingsEventArgs e)
		{
			// TODO: Load font info, recent files, last file, control contents. [Bill, 3/22/2022]
			Conditions.RequireReference(this, nameof(MainWindow));
		}

		private void FormSaverSaveSettings(object? sender, SettingsEventArgs e)
		{
			// TODO: Save font info, recent files, current file, control contents. [Bill, 3/22/2022]
			Conditions.RequireReference(this, nameof(MainWindow));
		}

		private void OutputTabIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			// When a visible tab becomes collapsed, its content is still visible until we select another tab.
			// See first comment under https://stackoverflow.com/a/12951255/1882616.
			if (sender is TabItem tab && !tab.IsVisible && this.bottomTabs.SelectedIndex > 0)
			{
				TabItem? lastItem = this.bottomTabs.Items.OfType<TabItem>().LastOrDefault(tab => tab.IsVisible);
				this.bottomTabs.SelectedIndex = lastItem != null ? this.bottomTabs.Items.IndexOf(lastItem) : 0;
			}
		}

		private void NewExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			this.Clear();
		}

		private void OpenExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (this.CanClear())
			{
				OpenFileDialog dialog = new();
				dialog.FileName = this.CurrentFileName;
				dialog.DefaultExt = FileDialogDefaultExt;
				dialog.Filter = FileDialogFilter;
				if (dialog.ShowDialog(this) ?? false)
				{
					this.Load(dialog.FileName, checkCanClear: false);
				}
			}
		}

		private void SaveExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			this.Save();
		}

		private void SaveAsExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			this.Save(true);
		}

		private void FontExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// WPF doesn't provide a common font dialog. The WinForms dialog is ugly but available.
			// https://stackoverflow.com/a/37578593/1882616
			using WinForms.FontDialog dialog = new()
			{
				AllowScriptChange = false,
				AllowSimulations = false,
				AllowVectorFonts = false,
				AllowVerticalFonts = false,
				FontMustExist = true,
#pragma warning disable MEN010 // Avoid magic numbers. Font point sizes are clear in context.
				MinSize = 8,
				MaxSize = 20,
#pragma warning restore MEN010 // Avoid magic numbers
				ScriptsOnly = true,
				ShowApply = false,
				ShowColor = false,
				ShowEffects = false,
				ShowHelp = false,
			};

			if (dialog.ShowDialog() == WinForms.DialogResult.OK)
			{
				ApplyFont(this.pattern, dialog.Font);
				ApplyFont(this.replacement, dialog.Font);
				ApplyFont(this.input, dialog.Font);
				ApplyFont(this.replaced, dialog.Font);
			}
		}

		private void InsertInlineOptionsExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			string positive = string.Empty;
			string negative = string.Empty;
			void AddOption(bool isEnabled, char inline)
			{
				if (isEnabled)
				{
					positive += inline;
				}
				else
				{
					negative += inline;
				}
			}

			AddOption(this.model.UseIgnoreCase, 'i');
			AddOption(this.model.UseMultiline, 'm');
			AddOption(this.model.UseSingleline, 's');
			AddOption(this.model.UseExplicitCapture, 'n');
			AddOption(this.model.UseIgnorePatternWhitespace, 'x');

			string inlineOptions = negative.IsEmpty() ? $"(?{positive})" : $"(?{positive}-{negative})";
			InsertText(this.pattern, inlineOptions);
		}

		private void GenerateCodeToClipboardExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish GenerateCodeToClipboardExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void InsertPatternExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string text)
			{
				InsertText(this.pattern, text);
				e.Handled = true;
			}
		}

		private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Model.IsModified):
					this.UpdateTitle();
					break;

				case nameof(Model.Pattern):
					if (!this.dirtyText.Contains(this.pattern))
					{
						this.TryQueueUpdate();
					}

					break;

				case nameof(Model.Replacement):
					if (!this.dirtyText.Contains(this.replacement))
					{
						this.TryQueueUpdate();
					}

					break;

				case nameof(Model.Input):
					if (!this.dirtyText.Contains(this.input))
					{
						this.TryQueueUpdate();
					}

					break;

				case nameof(Model.UnixNewline):
					if (this.updateState == UpdateState.None)
					{
						// How we split the lines changed, so we need to re-read the text boxes.
						this.dirtyText.Add(this.pattern);
						this.dirtyText.Add(this.replacement);
						this.dirtyText.Add(this.input);
						this.TryQueueUpdate();
					}

					break;

				default:
					// An option or a mode changed.
					if (this.updateState == UpdateState.None)
					{
						this.TryQueueUpdate();
					}

					break;
			}
		}

		private void EditorTextChanged(object sender, TextChangedEventArgs e)
		{
			if (this.updateState == UpdateState.None
				&& sender is RichTextBox richTextBox
				&& this.dirtyText.Add(richTextBox))
			{
				this.TryQueueUpdate();
			}
		}

		private void WindowClosing(object sender, CancelEventArgs e)
		{
			e.Cancel = !this.CanClear();
		}

		#endregion
	}
}
