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
	using System.Windows.Controls.Primitives;
	using System.Windows.Documents;
	using System.Windows.Input;

	#endregion

	// Based on https://prabu-guru.blogspot.com/2010/06/how-to-add-watermark-text-to-textbox.html
	// And on https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/Helper/TextBoxHelper.cs
	public class PlaceholderTextHelper : DependencyObject
	{
		#region Public Fields For Attached Properties

		public static readonly DependencyProperty IsMonitoringProperty =
			DependencyProperty.RegisterAttached(
				"IsMonitoring",
				typeof(bool),
				typeof(PlaceholderTextHelper),
				new UIPropertyMetadata(false, OnIsMonitoringChanged));

		public static readonly DependencyProperty PlaceholderTextProperty =
			DependencyProperty.RegisterAttached(
				"PlaceholderText",
				typeof(string),
				typeof(PlaceholderTextHelper),
				new UIPropertyMetadata(string.Empty));

		public static readonly DependencyProperty TextLengthProperty =
			DependencyProperty.RegisterAttached(
				"TextLength",
				typeof(int),
				typeof(PlaceholderTextHelper),
				new UIPropertyMetadata(0));

		public static readonly DependencyProperty HasTextProperty =
			DependencyProperty.RegisterAttached(
				"HasText",
				typeof(bool),
				typeof(PlaceholderTextHelper),
				new FrameworkPropertyMetadata(false, AffectsDisplay));

		#endregion

		#region Private Data Members

		private const FrameworkPropertyMetadataOptions AffectsDisplay = FrameworkPropertyMetadataOptions.AffectsMeasure
			| FrameworkPropertyMetadataOptions.AffectsArrange
			| FrameworkPropertyMetadataOptions.AffectsRender;

		#endregion

		#region Public Attached Properties

		public static bool GetIsMonitoring(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsMonitoringProperty);
		}

		public static void SetIsMonitoring(DependencyObject obj, bool value)
		{
			obj.SetValue(IsMonitoringProperty, value);
		}

		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static bool GetPlaceholderText(DependencyObject obj)
		{
			return (bool)obj.GetValue(PlaceholderTextProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static void SetPlaceholderText(DependencyObject obj, string value)
		{
			obj.SetValue(PlaceholderTextProperty, value);
		}

		public static int GetTextLength(DependencyObject obj)
		{
			return (int)obj.GetValue(TextLengthProperty);
		}

		public static void SetTextLength(DependencyObject obj, int value)
		{
			obj.SetValue(TextLengthProperty, value);
			obj.SetValue(HasTextProperty, value >= 1);
		}

		/// <summary>
		/// Gets if the attached TextBox has text.
		/// </summary>
		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static bool GetHasText(DependencyObject obj)
		{
			return (bool)obj.GetValue(HasTextProperty);
		}

		[AttachedPropertyBrowsableForType(typeof(TextBoxBase))]
		[AttachedPropertyBrowsableForType(typeof(PasswordBox))]
		public static void SetHasText(DependencyObject obj, bool value)
		{
			obj.SetValue(HasTextProperty, value);
		}

		#endregion

		#region Implementation

		private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is TextBoxBase txtBox)
			{
				if ((bool)e.NewValue)
				{
					txtBox.TextChanged += TextChanged;
				}
				else
				{
					txtBox.TextChanged -= TextChanged;
				}
			}
			else if (d is PasswordBox passBox)
			{
				if ((bool)e.NewValue)
				{
					passBox.PasswordChanged += PasswordChanged;
				}
				else
				{
					passBox.PasswordChanged -= PasswordChanged;
				}
			}
		}

		private static void TextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is TextBox txtBox)
			{
				SetTextLength(txtBox, txtBox.Text.Length);
			}
			else if (sender is RichTextBox richTextBox)
			{
				// This gets the text length of the first line.
				// From https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/Helper/TextBoxHelper.cs
				var textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
				var text = textRange.Text;
				var lastIndexOfNewLine = text.LastIndexOf(Environment.NewLine, StringComparison.InvariantCulture);
				if (lastIndexOfNewLine >= 0)
				{
					text = text.Remove(lastIndexOfNewLine);
				}

				SetTextLength(richTextBox, text.Length);
			}
		}

		private static void PasswordChanged(object sender, RoutedEventArgs e)
		{
			if (sender is PasswordBox passBox)
			{
				SetTextLength(passBox, passBox.Password.Length);
			}
		}

		#endregion
	}
}
