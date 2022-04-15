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
		private string currentFileName;
		private int updateLevel;

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

		private bool Load(string fileName, bool checkCanClear = true)
		{
			bool result = false;

			if (File.Exists(fileName) && (!checkCanClear || this.CanClear()))
			{
				this.CurrentFileName = FileUtility.ExpandFileName(fileName);
				this.model.Load(fileName);
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
					sb.Append(this.model.UnixNewline ? "\n" : "\r\n");
				}

				TextRange range = new(block.ContentStart, block.ContentEnd);
				string text = range.Text;
				sb.Append(text);
			}

			string result = sb.ToString();
			return result;
		}

		private IDisposable BeginUpdate()
		{
			Interlocked.Increment(ref this.updateLevel);
			return new Disposer(() =>
			{
				if (Interlocked.Increment(ref this.updateLevel) == 0)
				{
					this.EvalutateModel();
				}
			});
		}

		private void EvalutateModel()
		{
			const int TimeoutSeconds = 5;
			Evaluation evaluation = this.model.Evaluate(TimeSpan.FromSeconds(TimeoutSeconds));

			this.timing.Content = $"{evaluation.Elapsed.TotalMilliseconds} ms";
			this.message.Content = evaluation.Exception?.Message;

			this.matchGrid.ItemsSource = evaluation.Matches;
			this.replaced.Document = new FlowDocument(new Paragraph(new Run(evaluation.Replaced)));
			this.splitGrid.ItemsSource = evaluation.Splits;

			// TODO: Update this.pattern content and syntax highlight. [Bill, 4/9/2022]
			// TODO: Update this.replacement content and syntax highlight. [Bill, 4/9/2022]
			// TODO: Update this.input content and syntax highlight.  [Bill, 4/9/2022]
			// TODO: Populate matchGrid. [Bill, 4/15/2022]
			// TODO: Populate splitGrid. [Bill, 4/15/2022]
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
			if (this.CanClear())
			{
				this.CurrentFileName = string.Empty;
				this.model.Clear();
			}
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

				case nameof(Model.UnixNewline):
					using (this.BeginUpdate())
					{
						this.model.Pattern = this.GetText(this.pattern);
						this.model.Replacement = this.GetText(this.replacement);
						this.model.Input = this.GetText(this.input);
					}

					break;

				default:
					// Pattern, Replacement, Input, an option, or a mode changed.
					this.EvalutateModel();
					break;
			}
		}

		private void PatternTextChanged(object sender, TextChangedEventArgs e)
		{
			this.model.Pattern = this.GetText(this.pattern);
		}

		private void ReplacementTextChanged(object sender, TextChangedEventArgs e)
		{
			this.model.Replacement = this.GetText(this.replacement);
		}

		private void InputTextChanged(object sender, TextChangedEventArgs e)
		{
			this.model.Input = this.GetText(this.input);
		}

		#endregion
	}
}
