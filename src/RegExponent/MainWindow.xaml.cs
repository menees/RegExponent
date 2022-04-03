﻿namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
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

		private readonly Project project;
		private readonly WindowSaver saver;

		#endregion

		#region Constructors

		public MainWindow()
		{
			this.InitializeComponent();

			this.project = (Project)this.FindResource(nameof(Project));

			this.saver = new WindowSaver(this);
			this.saver.LoadSettings += this.FormSaverLoadSettings;
			this.saver.SaveSettings += this.FormSaverSaveSettings;

			// Only paste plain text and not rich text.
			DataObject.AddPastingHandler(this.pattern, OnPaste);
			DataObject.AddPastingHandler(this.replacement, OnPaste);
			DataObject.AddPastingHandler(this.input, OnPaste);

			// TODO: Handle SystemEvents.SessionEnding. If IsModified, then auto-save and close. [Bill, 3/31/2022]
		}

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
				e.Handled = true;
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
			OpenFileDialog dialog = new();
			dialog.ShowDialog();

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
			SaveFileDialog dialog = new();
			dialog.ShowDialog();

			// TODO: Finish SaveAsExecuted. [Bill, 3/31/2022]
			GC.KeepAlive(this);
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
				ApplyFont(this.replacedOutput, dialog.Font);
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

			AddOption(this.project.UseIgnoreCase, 'i');
			AddOption(this.project.UseMultiline, 'm');
			AddOption(this.project.UseSingleline, 's');
			AddOption(this.project.UseExplicitCapture, 'n');
			AddOption(this.project.UseIgnorePatternWhitespace, 'x');

			string inlineOptions = negative.IsBlank() ? $"(?{positive})" : $"(?{positive}-{negative})";
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

		#endregion
	}
}
