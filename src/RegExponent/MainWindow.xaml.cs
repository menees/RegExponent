namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Controls.Primitives;
	using System.Windows.Data;
	using System.Windows.Documents;
	using System.Windows.Input;
	using System.Windows.Interop;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Navigation;
	using System.Windows.Shapes;
	using System.Windows.Threading;
	using ICSharpCode.AvalonEdit;
	using ICSharpCode.AvalonEdit.Editing;
	using ICSharpCode.AvalonEdit.Highlighting;
	using Menees;
	using Menees.Windows.Presentation;
	using Microsoft.Win32;
	using Drawing = System.Drawing;
	using IO = System.IO;
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
		private const string TempExt = ".rgxtmp";

		private static readonly Regex InitialOptions = new(@"^(?n)\s*\(\?(?<options>([+\-]*[imnsx]*)+)[\:\)]", RegexOptions.Compiled);

		private readonly string[] commandLineArgs;
		private readonly Model model;
		private readonly WindowSaver saver;
		private readonly HashSet<TextEditor> dirtyText = new();
		private readonly Control[] customFontControls;
		private readonly ContextMenu recentDropDownMenu;
		private readonly RecentItemList<string> recentFiles;
		private readonly RichTextModel inputHighlight;
		private readonly HighlightingColor noHighlight = new();
		private readonly ObservableCollection<Benchmark> benchmarks = new();
		private string currentFileName;

		private int updateLevel;
		private bool updating;

		#endregion

		#region Constructors

		public MainWindow()
			: this(Array.Empty<string>())
		{
		}

		internal MainWindow(string[] commandLineArgs)
		{
			this.InitializeComponent();
			this.commandLineArgs = commandLineArgs;
			this.customFontControls = new Control[]
			{
				this.pattern,
				this.replacement,
				this.input,
				this.replaced,
				this.matchGrid,
				this.splitGrid,
				this.benchmarkGrid,
			};

			this.recentDropDownMenu = new ContextMenu
			{
				Placement = PlacementMode.Bottom,
				PlacementTarget = this.openButton,
			};

			foreach (Editor editor in this.customFontControls.OfType<Editor>())
			{
				editor.TextArea.GotFocus += (s, e) => this.UpdateSelectionDisplay(editor);
				editor.TextArea.Caret.PositionChanged += (s, e) => this.UpdateSelectionDisplay(editor);
			}

			this.model = (Model)this.FindResource(nameof(Model));
			this.model.PropertyChanged += this.ModelPropertyChanged;

			// Make sure this.Title and other fileName-related properties initialize properly.
			this.currentFileName = null!;
			this.CurrentFileName = string.Empty;

			this.saver = new WindowSaver(this);
			this.saver.LoadSettings += this.WindowSaverLoadSettings;
			this.saver.SaveSettings += this.WindowSaverSaveSettings;

			// Only paste plain text and not rich text.
			DataObject.AddPastingHandler(this.pattern, OnPaste);
			DataObject.AddPastingHandler(this.replacement, OnPaste);
			DataObject.AddPastingHandler(this.input, OnPaste);

			this.recentFiles = new(this.recentMainMenu, this.RecentFileClick, this.recentDropDownMenu);

			// https://github.com/icsharpcode/AvalonEdit/issues/244#issuecomment-725214919
			this.inputHighlight = new();
			this.input.TextArea.TextView.LineTransformers.Add(new RichTextColorizer(this.inputHighlight));

			SystemEvents.SessionEnding += this.SystemEventsSessionEnding;

			// We can easily set the sort direction indicator in XAML, but that doesn't actually sort.
			// This sets the initial sort. https://stackoverflow.com/a/9168101/1882616
			this.benchmarkGrid.ItemsSource = this.benchmarks;
			ICollectionView benchmarkView = CollectionViewSource.GetDefaultView(this.benchmarkGrid.ItemsSource);
			benchmarkView.SortDescriptions.Add(new SortDescription(nameof(Benchmark.Index), ListSortDirection.Descending));
			this.benchmarksTab.Visibility = Visibility.Collapsed;
		}

		#endregion

		#region Private Enums

		private enum Code
		{
			Pattern,
			Replacement,
			Input,
			Block,
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
					// Ignore temp files so they don't show in the title or in the recent file list.
					if (!IsTempFile(value))
					{
						this.currentFileName = value;
						this.UpdateTitle();

						if (value.IsNotEmpty())
						{
							this.recentFiles.Add(value);
						}
					}
				}
			}
		}

		private string ModelDisplayName =>
			this.CurrentFileName.IsNotEmpty()
			? IO.Path.GetFileNameWithoutExtension(this.CurrentFileName)
			: "<Untitled>";

		private Editor CurrentEditor
		{
			get
			{
				Editor result = this.replaced.TextArea.IsFocused ? this.replaced
					: this.replacement.TextArea.IsFocused ? this.replacement
					: this.pattern.TextArea.IsFocused ? this.pattern
					: this.input;
				return result;
			}
		}

		#endregion

		#region Private Methods

		private static bool IsTempFile(string fileName)
		{
			bool result = fileName.IsNotEmpty()
				&& IO.Path.GetExtension(fileName).Equals(TempExt, StringComparison.OrdinalIgnoreCase)
				&& (IO.Path.GetDirectoryName(fileName) ?? string.Empty).Equals(GetTempFolder(false), StringComparison.OrdinalIgnoreCase);
			return result;
		}

		private static void InsertText(TextEditor editor, string text)
		{
			editor.SelectedText = text;
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

		private static string GetTempFolder(bool ensureExists)
		{
			// Even though this isn't for user-and-system-specific data, I'm going to use the LocalAppData folder
			// instead of (roaming) AppData because this is just for local temp data. If the user really wants the
			// data persisted and roamed, they can save it to a roaming location themselves (e.g., OneDrive).
			// Also, modern recommendations from MS are to reduce profile bloat whenever possible. I'm not using
			// the user's temp path because it's a shared use temporary folder. Using LocalAppData is a little more
			// private and less likely to get purged between runs.
			// https://danv74.wordpress.com/2020/11/23/appdata-and-localappdata/
			// https://techcommunity.microsoft.com/t5/windows-it-pro-blog/windows-10-roaming-settings-for-remote-work-scenarios/ba-p/1544100
			string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string result = IO.Path.Combine(baseFolder, nameof(RegExponent));
			if (ensureExists)
			{
				Directory.CreateDirectory(result);
			}

			return result;
		}

		private static void SetColumnVisibility<T>(DataGridColumn column, IEnumerable<T> items, Predicate<T> isVisible)
		{
			column.Visibility = items.Any(item => isVisible(item)) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void ApplyFont(string familyName, double size, FontStyle style, FontWeight weight)
		{
			FontFamily fontFamily = new(familyName);
			foreach (Control control in this.customFontControls)
			{
				control.FontFamily = fontFamily;
				control.FontSize = size;
				control.FontStyle = style;
				control.FontWeight = weight;
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

		private IDisposable BeginUpdate()
		{
			bool previous = this.updating;
			this.updating = true;
			return new Disposer(() => this.updating = previous);
		}

		private void Clear()
		{
			if (this.CanClear())
			{
				this.CurrentFileName = string.Empty;
				this.benchmarks.Clear();
				using (this.BeginUpdate())
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
				fileName = FileUtility.ExpandFileName(fileName);
				using (this.BeginUpdate())
				{
					this.model.Load(fileName);
					this.CurrentFileName = fileName;
				}

				this.TryQueueUpdate();
				result = true;
			}

			return result;
		}

		private bool Save(bool forceSaveDialog = false)
		{
			bool trySave = true;

			string fileName = this.CurrentFileName;
			if (forceSaveDialog || fileName.IsEmpty())
			{
				SaveFileDialog dialog = new()
				{
					FileName = fileName,
					DefaultExt = FileDialogDefaultExt,
					Filter = FileDialogFilter,
				};

				trySave = dialog.ShowDialog(this) ?? false;
				if (trySave)
				{
					fileName = dialog.FileName;
				}
			}

			bool saved = false;
			if (trySave && fileName.IsNotEmpty())
			{
				this.SaveAs(fileName);
				saved = true;
			}

			return saved;
		}

		private void SaveAs(string fileName, bool isTempFile = false)
		{
			this.model.Save(fileName, isTempFile);
			this.CurrentFileName = fileName;
		}

		private void UpdateTitle()
		{
			// Put the model display name first so it's easier to distinguish multiple instances on the taskbar (like Word and Excel do).
			StringBuilder sb = new(this.ModelDisplayName);
			sb.Append(" - ");
			sb.Append(nameof(RegExponent));
			if (this.model.IsModified)
			{
				sb.Append(" - ");
				sb.Append("Modified");
			}

			this.Title = sb.ToString();
		}

		private void TryQueueUpdate()
		{
			if (!this.updating)
			{
				this.Dispatcher.BeginInvoke(new Action(this.BeginForegroundUpdate), DispatcherPriority.ApplicationIdle);
			}
		}

		private Evaluator CreateEvaluator()
		{
			int currentUpdateLevel = Interlocked.Increment(ref this.updateLevel);
			const int TimeoutSeconds = 5;
			Evaluator evaluator = new(this.model, TimeSpan.FromSeconds(TimeoutSeconds), currentUpdateLevel);
			return evaluator;
		}

		private void BeginForegroundUpdate()
		{
			// Note: We can't call bool Remove(...) first because we need the control to
			// remain in the dirty set while the model's PropertyChanged event is handled.
			if (this.dirtyText.Contains(this.pattern))
			{
				this.model.Pattern = this.pattern.GetText();
				this.dirtyText.Remove(this.pattern);
			}

			if (this.dirtyText.Contains(this.replacement))
			{
				this.model.Replacement = this.replacement.GetText();
				this.dirtyText.Remove(this.replacement);
			}

			if (this.dirtyText.Contains(this.input))
			{
				this.model.Input = this.input.GetText();
				this.dirtyText.Remove(this.input);
			}

			Evaluator evaluator = this.CreateEvaluator();

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
			// Note: This needs to run even if Evaluate did nothing because we need New() to reset all the controls.
			if (evaluator.UpdateLevel == this.updateLevel)
			{
				using (this.BeginUpdate())
				{
					this.pattern.SetText(this.model.Pattern);
					this.input.SetText(this.model.Input);
					Dictionary<Match, Color> matchColors = this.UpdateInputHighlight(evaluator.Matches);

					this.timing.Content = $"{evaluator.Elapsed.TotalMilliseconds} ms";
					string? message = evaluator.Exception?.Message;
					this.message.Content = message.IsEmpty() ? null : new TextBlock(new Run(message)) { TextWrapping = TextWrapping.Wrap };

					List<MatchModel> matches = evaluator.Matches
						.SelectMany((match, matchNum) => match.Groups.Cast<Group>()
							.Select((group, groupNum) => (matchNum, match, groupNum, group))
							.Where(tuple => tuple.group.Success))
						.Select(tuple => new MatchModel(
							tuple.groupNum == 0 ? tuple.matchNum : null,
							tuple.groupNum != 0 ? tuple.group.Name : null,
							tuple.group.Index,
							tuple.group.Length,
							tuple.group.Value,
							matchColors.TryGetValue(tuple.match, out Color color) ? color : null))
						.ToList();
					SetColumnVisibility(this.groupColumn, matches, m => m.Group.IsNotEmpty());
					this.matchGrid.ItemsSource = matches;
					this.matchTab.Header = $"Matches ({evaluator.Matches.Count})";

					if (this.model.InReplaceMode)
					{
						this.replacement.SetText(this.model.Replacement);
						this.replaced.SetText(evaluator.Replaced);
					}
					else if (this.model.InSplitMode)
					{
						static string GetInterestingLiteral(string text)
						{
							string literal = CodeGenerator.ToLiteral(text);
							return literal == '"' + text.Trim() + '"' ? string.Empty : literal;
						}

						var splits = evaluator.Splits
							.Select((line, index) => new
							{
								Index = index,
								Value = line,
								line?.Length,
								Comment = line == null ? "null"
									: line.Length == 0 ? "Empty"
									: GetInterestingLiteral(line),
							}).ToList();
						SetColumnVisibility(this.splitCommentColumn, splits, s => s.Comment.IsNotEmpty());
						this.splitGrid.ItemsSource = splits;
						this.splitTab.Header = $"Output ({splits.Count})";
					}
				}
			}
		}

		private void ClearInputHighlight()
		{
			// Reset beyond the current document's text length in case the previous
			// document was longer than the current document.
			this.inputHighlight.SetHighlighting(0, int.MaxValue, this.noHighlight);
		}

		private Dictionary<Match, Color> UpdateInputHighlight(IReadOnlyList<Match> matches)
		{
			// First, remove any prior colors because the pattern or the input could have changed.
			this.ClearInputHighlight();

			// https://github.com/icsharpcode/AvalonEdit/issues/244#issuecomment-725214919
			// Next, apply highlights for the new matches.
			Color yellow = Colors.LemonChiffon;
			Color orange = Colors.NavajoWhite;
			SimpleHighlightingBrush yellowBrush = new(yellow);
			SimpleHighlightingBrush orangeBrush = new(orange);
			SimpleHighlightingBrush backgroundBrush = yellowBrush;

			Dictionary<Match, Color> result = new(matches.Count);
			foreach (Match match in matches.Where(m => m.Success))
			{
				result.Add(match, backgroundBrush == yellowBrush ? yellow : orange);
				this.inputHighlight.SetBackground(match.Index, match.Length, backgroundBrush);
				backgroundBrush = backgroundBrush == yellowBrush ? orangeBrush : yellowBrush;

				foreach (Group group in match.Groups.Cast<Group>().Skip(1).Where(g => g.Success))
				{
					this.inputHighlight.SetFontWeight(group.Index, group.Length, FontWeights.Bold);
				}
			}

			this.input.TextArea.TextView.Redraw(DispatcherPriority.Render);
			return result;
		}

		private void SelectLastVisibleBottomTab()
		{
			TabItem? lastItem = this.bottomTabs.Items.OfType<TabItem>().LastOrDefault(tab => tab.IsVisible);
			this.bottomTabs.SelectedIndex = lastItem != null ? this.bottomTabs.Items.IndexOf(lastItem) : 0;
		}

		private Editor? GetCodeEditor(Code code)
			=> code switch
			{
				Code.Pattern => this.pattern,
				Code.Replacement => this.replacement,
				Code.Input => this.input,
				_ => null,
			};

		private void GenerateCSharp(Code code)
		{
			Editor? editor = this.GetCodeEditor(code);

			string text = editor != null
				? CodeGenerator.ToVerbatimLines(editor.Text, this.model.Newline)
				: CodeGenerator.GenerateBlock(this.model);

			Clipboard.SetText(text, TextDataFormat.UnicodeText);
		}

		private void GenerateHtml(Code code)
		{
			Editor? editor = this.GetCodeEditor(code);
			if (editor != null)
			{
				HtmlOptions options = new();
				string html;
				if (editor == this.input)
				{
					// LONG-TERM-TODO: Use AvalonEdit NuGet package after https://github.com/icsharpcode/AvalonEdit/pull/342 released. [Bill, 5/10/2022]
					RichText richText = new(editor.Text, this.inputHighlight);
					html = richText.ToHtml(options);
				}
				else
				{
					IHighlighter? highlighter = editor.TextArea.GetService(typeof(IHighlighter)) as IHighlighter;
					html = HtmlClipboard.CreateHtmlFragment(editor.Document, highlighter, null, options);
				}

				DataObject dataObject = new(html);
				HtmlClipboard.SetHtml(dataObject, html);
				Clipboard.SetDataObject(dataObject);
			}
		}

		private void UpdateSelectionDisplay(Editor editor)
		{
			if (editor == this.CurrentEditor)
			{
				// We can't trust editor.SelectionStart or editor.SelectionLength here when this method is called from the
				// Caret.PositionChanged event since those properties are only updated after that event is raised.
				// So, we'll post a message to update after all the caret and selection properties are up-to-date.
				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					if (editor.SelectionLength == 0)
					{
						this.selectionDisplay.Content = $"@ {editor.TextArea.Caret.Offset}";
					}
					else
					{
						this.selectionDisplay.Content = $"S {editor.SelectionStart} L {editor.SelectionLength}";
					}
				}));
			}
		}

		private void BeginRunBenchmark()
		{
			// Use a temporary begin benchmark for displaying a status. This won't be touched
			// by any worker threads, and it will be replaced by EndRunBenchmark.
			Benchmark benchmark = new() { Index = this.benchmarks.Count + 1 };
			benchmark.Comment = $"Running for {benchmark.FormattedTimeout}...";
			this.benchmarks.Add(benchmark);
			this.ShowBenchmarks();

			Evaluator evaluator = this.CreateEvaluator();

			// See comments in this.BeginForegroundUpdate() for why we use Task.Run.
			Task.Run(() =>
			{
				evaluator.RunBenchmark(() => this.updateLevel);
				this.Dispatcher.BeginInvoke(
					new Action<Evaluator, Benchmark>(this.EndRunBenchmark), DispatcherPriority.ApplicationIdle, evaluator, benchmark);
			});
		}

		private void EndRunBenchmark(Evaluator evaluator, Benchmark beginBenchmark)
		{
			Benchmark endBenchmark;
			if (evaluator.Exception != null)
			{
				endBenchmark = new() { Comment = evaluator.Exception.Message };
			}
			else if (evaluator.Benchmark == null)
			{
				endBenchmark = new() { Comment = "Run benchmark requires a pattern." };
			}
			else
			{
				endBenchmark = evaluator.Benchmark;
			}

			// Replace beginBenchmark with endBenchmark so the grid knows to update.
			int index = this.benchmarks.IndexOf(beginBenchmark);
			if (index >= 0)
			{
				endBenchmark.Index = beginBenchmark.Index;
				this.benchmarks[index] = endBenchmark;
				SetColumnVisibility(this.benchmarkReplace, this.benchmarks, b => b.ReplaceCount != null);
				SetColumnVisibility(this.benchmarkSplit, this.benchmarks, b => b.SplitCount != null);
				this.ShowBenchmarks();
			}
		}

		private void ShowBenchmarks()
		{
			this.benchmarksTab.Visibility = Visibility.Visible;
			this.bottomTabs.SelectedItem = this.benchmarksTab;
		}

		private bool SetPatternHighligher()
		{
			// If the pattern toggles x-mode on or off, then that has precedence over RegexOptions.
			// We only support one highlighting mode for the whole pattern, so we'll only look for x-mode
			// options at the start of the pattern. The mode can change mid-pattern with inline options,
			// but we won't try to handle that for syntax highlighting.
			bool? useXMode = null;
			Match match = InitialOptions.Match(this.model.Pattern);
			if (match.Success && match.Groups.Count == 2)
			{
				// For options the last referenced state wins, e.g., (?+xxxxxmisn---x-ixmsixx-x) ends with x-mode off.
				string options = match.Groups[1].Value;
				bool enable = true; // Assume '+' until we see a '-'.
				foreach (char ch in options)
				{
					switch (ch)
					{
						case 'x':
							useXMode = enable;
							break;

						case '+':
							enable = true;
							break;

						case '-':
							enable = false;
							break;
					}
				}
			}

			// If initial options didn't explicitly set x-mode on or off, then fall back to the model's RegexOptions.
			useXMode ??= this.model.UseIgnorePatternWhitespace;

			IHighlightingDefinition highlighting = HighlightingManager.Instance.GetDefinition(useXMode.Value ? "PatternXMode" : "Pattern");

			bool changed = false;
			if (this.pattern.SyntaxHighlighting != highlighting)
			{
				this.pattern.SyntaxHighlighting = highlighting;
				changed = true;
			}

			return changed;
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

		private void WindowSourceInitialized(object sender, EventArgs e)
		{
			// This can't go in the constructor or Load event because the CommandParameter bindings return null there.
			// See the <Window.InputBindings> element in MainWindow.xaml for why this is necessary.
			foreach (KeyBinding keyBinding in this.InputBindings.OfType<KeyBinding>())
			{
				if (keyBinding.CommandParameter is MenuItem menuItem && keyBinding.Gesture is KeyGesture gesture)
				{
					menuItem.InputGestureText = gesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
				}
			}
		}

		private void WindowSaverLoadSettings(object? sender, SettingsEventArgs e)
		{
			ISettingsNode settings = e.SettingsNode;

			Control control = this.customFontControls[0];
			string fontFamily = settings.GetValue("Font.Family", control.FontFamily.Source);
			if (!double.TryParse(settings.GetValue("Font.Size", control.FontSize.ToString()), out double fontSize))
			{
				fontSize = control.FontSize;
			}

			FontStyle fontStyle = settings.GetValue("Font.Style", control.FontStyle);
			FontWeight fontWeight = settings.GetValue("Font.Weight", control.FontWeight);
			this.ApplyFont(fontFamily, fontSize, fontStyle, fontWeight);

			this.recentFiles.Load(settings, RecentItemList<string>.LoadString);
			WindowSaver.LoadSplits(settings, this.splitter);

			string loadFileName = settings.GetValue(nameof(this.CurrentFileName), string.Empty);
			if (this.commandLineArgs.Length == 1)
			{
				// Clean up the previous auto-save temp file since we're not going to use it.
				if (IsTempFile(loadFileName))
				{
					FileUtility.TryDeleteFile(loadFileName);
				}

				loadFileName = this.commandLineArgs[0];
			}

			if (loadFileName.IsNotEmpty())
			{
				this.Dispatcher.BeginInvoke(new Action(() =>
				{
					if (this.Load(loadFileName, checkCanClear: false) && IsTempFile(loadFileName))
					{
						FileUtility.TryDeleteFile(loadFileName);

						// Set as modified so New and Open will know to prompt before clearing changes,
						// and so we'll know to auto-save the settings on close if there's no filename.
						this.model.IsModified = true;
					}
				}));
			}
		}

		private void WindowSaverSaveSettings(object? sender, SettingsEventArgs e)
		{
			ISettingsNode settings = e.SettingsNode;

			Control control = this.customFontControls[0];
			settings.SetValue("Font.Family", control.FontFamily.Source);
			settings.SetValue("Font.Size", control.FontSize.ToString());
			settings.SetValue("Font.Style", control.FontStyle);
			settings.SetValue("Font.Weight", control.FontWeight);

			this.recentFiles.Save(settings, RecentItemList<string>.SaveString);
			WindowSaver.SaveSplits(settings, this.splitter);

			string saveFileName = this.CurrentFileName;

			// If there's no current file name, then save everything to a temp file.
			if (saveFileName.IsEmpty() && this.model.IsModified)
			{
				// Save to a unique temp file name in case the user runs multiple copies of RegExponent from
				// different directories. Then each copy's settings store will have its own temp auto-save file.
				string tempFolder = GetTempFolder(true);
				saveFileName = FileUtility.GetTempFileName(TempExt, tempFolder);
				this.SaveAs(saveFileName, isTempFile: true);
			}

			settings.SetValue(nameof(this.CurrentFileName), saveFileName);
		}

		private void OutputTabIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
		{
			// When a visible tab becomes collapsed, its content is still visible until we select another tab.
			// See first comment under https://stackoverflow.com/a/12951255/1882616.
			if (sender is TabItem tab && !tab.IsVisible && this.bottomTabs.SelectedIndex > 0)
			{
				this.SelectLastVisibleBottomTab();
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
				OpenFileDialog dialog = new()
				{
					FileName = this.CurrentFileName,
					DefaultExt = FileDialogDefaultExt,
					Filter = FileDialogFilter,
				};

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

			Control control = this.customFontControls[0];
			string familyName = control.FontFamily.Source;
			Drawing.FontStyle fontStyle = Drawing.FontStyle.Regular;
			if (control.FontWeight == FontWeights.Bold)
			{
				fontStyle |= Drawing.FontStyle.Bold;
			}

			if (control.FontStyle == FontStyles.Italic)
			{
				fontStyle |= Drawing.FontStyle.Italic;
			}

			using Drawing.Font priorFont = new(familyName, (float)control.FontSize, fontStyle, Drawing.GraphicsUnit.Pixel);
			dialog.Font = priorFont;

			// https://stackoverflow.com/a/10296513/1882616
			WinForms.NativeWindow nativeWindow = new();
			nativeWindow.AssignHandle(new WindowInteropHelper(this).Handle);
			if (dialog.ShowDialog(nativeWindow) == WinForms.DialogResult.OK)
			{
				// From https://stackoverflow.com/a/37578593/1882616
				Drawing.Font font = dialog.Font;
				double fontSize = control.FontSize;
				switch (font.Unit)
				{
					case Drawing.GraphicsUnit.Point:
						const double XamlPixelsPerInch = 96.0;
						const double DrawingPointsPerInch = 72.0;
						fontSize = font.Size * XamlPixelsPerInch / DrawingPointsPerInch;
						break;

					case Drawing.GraphicsUnit.Pixel:
						fontSize = font.Size;
						break;
				}

				this.ApplyFont(
					font.Name,
					fontSize,
					font.Italic ? FontStyles.Italic : FontStyles.Normal,
					font.Bold ? FontWeights.Bold : FontWeights.Regular);
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
			using (this.BeginUpdate())
			{
				InsertText(this.pattern, inlineOptions);
			}
		}

		private void GenerateCodeToClipboardExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string parameter)
			{
				string[] args = parameter.Split('/');
				if (args.Length == 2 && Enum.TryParse(args[1], out Code code))
				{
					string language = args[0];
					switch (language)
					{
						case "C#":
							this.GenerateCSharp(code);
							break;

						case "HTML":
							this.GenerateHtml(code);
							break;
					}
				}
			}
		}

		private void InsertPatternExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string text)
			{
				e.Handled = true;
				using (this.BeginUpdate())
				{
					InsertText(this.pattern, text);
				}
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
					// Editing the pattern can change the inline x-mode state.
					if (this.SetPatternHighligher() || !this.dirtyText.Contains(this.pattern))
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
					this.ClearInputHighlight();
					if (!this.dirtyText.Contains(this.input))
					{
						this.TryQueueUpdate();
					}

					break;

				case nameof(Model.WindowsNewline):
				case nameof(Model.UnixNewline):
					if (!this.updating)
					{
						// How we split the lines changed, so we need to re-read the text boxes.
						this.dirtyText.Add(this.pattern);
						this.dirtyText.Add(this.replacement);
						this.dirtyText.Add(this.input);
						this.TryQueueUpdate();
					}

					this.newlineDisplay.Content = this.model.Newline.Replace("\r", "\\r").Replace("\n", "\\n");
					break;

				case nameof(Model.InMatchMode):
				case nameof(Model.InReplaceMode):
				case nameof(Model.InSplitMode):
					this.Dispatcher.BeginInvoke(new Action(() => this.SelectLastVisibleBottomTab()));
					goto default;

				case nameof(Model.UseIgnorePatternWhitespace):
					if (this.SetPatternHighligher())
					{
						goto default;
					}

					break;

				default:
					// An option or a mode changed.
					this.TryQueueUpdate();
					break;
			}
		}

#pragma warning disable CC0068 // Unused Method. Event handler referenced in XAML.
		private void EditorTextChanged(object sender, EventArgs e)
#pragma warning restore CC0068 // Unused Method
		{
			if (!this.updating
				&& sender is TextEditor editor
				&& this.dirtyText.Add(editor))
			{
				this.TryQueueUpdate();
			}
		}

		private void WindowClosing(object sender, CancelEventArgs e)
		{
			// If there's no current file name, then the FormSaver events will implicitly save to a
			// temp file and then reload the current data from the temp file on the next run.
			// So we only need to prompt about unsaved changes if there is an explicit file name.
			e.Cancel = this.CurrentFileName.IsNotEmpty() && !this.CanClear();
		}

		private void DropDownRecentItemsClick(object sender, RoutedEventArgs e)
		{
			this.recentDropDownMenu.IsOpen = true;
		}

		private void RecentFileClick(string recentFile)
		{
			if (!this.Load(recentFile) && !this.model.IsModified)
			{
				if (WindowsUtility.ShowQuestion(this, $"Could not load:\n\n\"{recentFile}\"\n\nDo you want to remove it from the recent items?"))
				{
					this.recentFiles.Remove(recentFile);
				}
			}
		}

		private void SystemEventsSessionEnding(object sender, SessionEndingEventArgs e)
		{
			// If the session is ending (due to user logoff or system shutdown),
			// then quietly save any changes to the current file. I'd rather err on
			// the side of preserving the most recent edits than to pop up a
			// modal confirmation dialog during shutdown.
			if (this.CurrentFileName.IsNotEmpty() && this.model.IsModified)
			{
				this.Save();
			}

			this.Close();
		}

		private void MessageDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (sender is ContentControl contentControl && contentControl.Content is string text)
			{
				Match match = Regex.Match(text, @"(?n)at offset (?<offset>\d+)[^\d]");
				if (match.Success
					&& match.Groups.Count == 2
					&& int.TryParse(match.Groups[1].Value, out int offset)
					&& offset >= 0
					&& offset <= this.pattern.Document.TextLength)
				{
					this.pattern.CaretOffset = offset;
					this.pattern.TextArea.Caret.BringCaretToView();
					this.pattern.Focus();
				}
			}
		}

#pragma warning disable CC0091 // Use static method. Xaml expects non-static event handler.
		private void ToggleMenuItemExecuted(object sender, ExecutedRoutedEventArgs e)
#pragma warning restore CC0091 // Use static method
		{
			if (e.Parameter is MenuItem menuItem)
			{
				menuItem.IsChecked = !menuItem.IsChecked;
			}
		}

		private void NewlineDisplayDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (this.model.UnixNewline)
			{
				this.model.WindowsNewline = true;
			}
			else
			{
				this.model.UnixNewline = true;
			}
		}

		private void MatchGridCurrentCellChanged(object sender, EventArgs e)
		{
			if (this.matchGrid.CurrentCell.Item is MatchModel matchModel
				&& matchModel.Index >= 0 && (matchModel.Index + matchModel.Length) <= this.input.Document.TextLength)
			{
				this.input.Select(matchModel.Index, matchModel.Length);
			}
		}

		private void WindowDrop(object sender, DragEventArgs e)
		{
			// https://stackoverflow.com/a/5663329/1882616
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				if (files.Length == 1)
				{
					// Post a message to try the file open so the OLE drop call can finish first.
					// We never want a modal "CanClear" dialog to block the OLE drag-drop handler.
					this.Dispatcher.BeginInvoke(new Action(() => this.Load(files[0])));
				}
			}
		}

		private void RunBenchmarkExecuted(object sender, ExecutedRoutedEventArgs e)
			=> this.BeginRunBenchmark();

		private void CloseBenchmarksClick(object sender, RoutedEventArgs e)
		{
			this.benchmarksTab.Visibility = Visibility.Collapsed;
		}

		private void ViewBenchmarksExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			this.ShowBenchmarks();
		}

		#endregion

		#region Private Types

		private sealed record MatchModel(int? Match, string? Group, int Index, int Length, string Value, Color? Color)
		{
			// This property is used by XAML since triggers can't match "not null".
			public bool UseMatchBrush => this.Match != null && this.Color != null;

			// Return a brush instead of a color to prevent WPF data binding warnings with creating a brush in XAML.
			// https://stackoverflow.com/a/7926385/1882616
			public Brush MatchBrush { get; } = new SolidColorBrush(Color != null ? Color.Value : Colors.Transparent);
		}

		#endregion
	}
}
