namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
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

	#endregion

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		#region Private Data Members

		private readonly WindowSaver saver;

		#endregion

		#region Constructors

		public MainWindow()
		{
			this.InitializeComponent();

			this.saver = new WindowSaver(this);
			this.saver.LoadSettings += this.FormSaverLoadSettings;
			this.saver.SaveSettings += this.FormSaverSaveSettings;

			// Only paste plain text and not rich text.
			DataObject.AddPastingHandler(this.pattern, OnPaste);
			DataObject.AddPastingHandler(this.replacement, OnPaste);
			DataObject.AddPastingHandler(this.input, OnPaste);
		}

		#endregion

		#region Private Event Handlers

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

		private void ExitExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void ShellExecuteExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string text && Uri.TryCreate(text, UriKind.Absolute, out Uri? uri))
			{
				WindowsUtility.ShellExecute(this, uri.ToString());
			}
		}

		private void AboutExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			WindowsUtility.ShowAboutBox(this, this.GetType().Assembly, nameof(RegExponent));
		}

		private void FormSaverLoadSettings(object? sender, SettingsEventArgs e)
		{
			// TODO: Finish FormSaverLoadSettings. [Bill, 3/22/2022]
			Conditions.RequireReference(this, nameof(MainWindow));
		}

		private void FormSaverSaveSettings(object? sender, SettingsEventArgs e)
		{
			// TODO: Finish FormSaverSaveSettings. [Bill, 3/22/2022]
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
			// TODO: Finish NewExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void OpenExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish OpenExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void SaveExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish SaveExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void SaveAsExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish SaveAsExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void FontExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish FontExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void InsertInlineOptionsExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish InsertInlineOptionsExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void GenerateCodeToClipboardExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish GenerateCodeToClipboardExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		private void InsertPatternExecuted(object? sender, ExecutedRoutedEventArgs e)
		{
			// TODO: Finish InsertPatternExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
		}

		#endregion
	}
}
