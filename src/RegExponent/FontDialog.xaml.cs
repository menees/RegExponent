namespace RegExponent;

#region Using Directives

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

#endregion

public partial class FontDialog : Window
{
	#region Private Data Members

	private ICollection<FontFamily> fontFamilies;
	private ICollection<FontStyle> fontStyles;
	private ICollection<FontWeight> fontWeights;
	private ICollection<double> fontSizes;

	#endregion

	#region Constructors

	public FontDialog()
	{
		this.InitializeComponent();

		this.FontFamilies = [.. Fonts.SystemFontFamilies.OrderBy(family => family.ToString())];

		this.FontStyles =
		[
			System.Windows.FontStyles.Normal,
			System.Windows.FontStyles.Oblique,
			System.Windows.FontStyles.Italic,
		];

		this.FontWeights =
		[
			System.Windows.FontWeights.Thin,
			System.Windows.FontWeights.Light,
			System.Windows.FontWeights.Regular,
			System.Windows.FontWeights.Normal,
			System.Windows.FontWeights.Medium,
			System.Windows.FontWeights.Heavy,
			System.Windows.FontWeights.SemiBold,
			System.Windows.FontWeights.DemiBold,
			System.Windows.FontWeights.Bold,
			System.Windows.FontWeights.Black,
			System.Windows.FontWeights.ExtraLight,
			System.Windows.FontWeights.ExtraBold,
			System.Windows.FontWeights.ExtraBlack,
			System.Windows.FontWeights.UltraLight,
			System.Windows.FontWeights.UltraBold,
			System.Windows.FontWeights.UltraBlack,
		];

#pragma warning disable MEN010 // Avoid magic numbers. Standard font sizes are clear in context.
		this.FontSizes ??= [8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72, 84, 96, 108, 144, 192, 216, 288];
#pragma warning restore MEN010 // Avoid magic numbers
	}

	#endregion

	#region Public Properties

	/// <summary>
	/// Gets or sets a collection of avaliable font styles.
	/// </summary>
	public ICollection<FontFamily> FontFamilies
	{
		get => this.fontFamilies;

		[MemberNotNull(nameof(this.fontFamilies))]
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			if (this.fontFamilies != value)
			{
				this.fontFamilies = value;
				this.fontFamilyListBox.ItemsSource = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets a collection of avaliable font styles.
	/// </summary>
	public ICollection<FontStyle> FontStyles
	{
		get => this.fontStyles;

		[MemberNotNull(nameof(this.fontStyles))]
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			if (this.fontStyles != value)
			{
				this.fontStyles = value;
				this.fontStyleListBox.ItemsSource = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets a collection of avaliable font weights.
	/// </summary>
	public ICollection<FontWeight> FontWeights
	{
		get => this.fontWeights;

		[MemberNotNull(nameof(this.fontWeights))]
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			if (this.fontWeights != value)
			{
				this.fontWeights = value;
				this.fontWeightListBox.ItemsSource = value;
			}
		}
	}

	/// <summary>
	/// Gets or sets a collection of available font sizes.
	/// </summary>
	public ICollection<double> FontSizes
	{
		get => this.fontSizes;

		[MemberNotNull(nameof(this.fontSizes))]
		set
		{
			ArgumentNullException.ThrowIfNull(value);
			if (this.fontSizes != value)
			{
				this.fontSizes = value;
				this.fontSizeListBox.ItemsSource = value;
			}
		}
	}

	public FontFamily SelectedFontFamily
	{
		get => (FontFamily)this.fontFamilyListBox.SelectedItem;
		set => this.SelectListItem(this.fontFamilyListBox, value);
	}

	public FontStyle SelectedFontStyle
	{
		get => (FontStyle)this.fontStyleListBox.SelectedItem;
		set => this.SelectListItem(this.fontStyleListBox, value);
	}

	public FontWeight SelectedFontWeight
	{
		get => (FontWeight)this.fontWeightListBox.SelectedItem;
		set => this.SelectListItem(this.fontWeightListBox, value);
	}

	public double SelectedFontSize
	{
		get => (double)this.fontSizeListBox.SelectedItem;
		set => this.SelectListItem(this.fontSizeListBox, value);
	}

	#endregion

	#region Protected Methods

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		NativeMethods.RemoveIcon(this);
	}

	#endregion

	#region Private Methods

	private static bool TryParseFontInfo<TConverter, TInfo>(string fontStyleName, out TInfo fontInfo)
		where TConverter : TypeConverter, new()
		where TInfo : struct
	{
		bool result = false;
		fontInfo = default;

		try
		{
			var converter = new TConverter();
			if (converter.CanConvertFrom(typeof(string))
				&& converter.ConvertFromString(fontStyleName) is TInfo info)
			{
				fontInfo = info;
				result = true;
			}
		}
#pragma warning disable CC0004 // Catch block cannot be empty
		catch (Exception ex) when (ex is FormatException or NotSupportedException)
		{
		}
#pragma warning restore CC0004 // Catch block cannot be empty

		return result;
	}

	private static ListBoxItem? FocusSelectedItem(ListBox listBox)
	{
		ListBoxItem? result = null;

		if (listBox.SelectedItem is object selectedItem)
		{
			listBox.UpdateLayout();
			if (listBox.ItemContainerGenerator.ContainerFromItem(selectedItem) is ListBoxItem container)
			{
				container.Focus();
				result = container;
			}
		}

		return result;
	}

	private void SelectListItem(ListBox listBox, object value)
	{
		listBox.SelectedItem = value;
		listBox.ScrollIntoView(listBox.SelectedItem);
		this.Dispatcher.BeginInvoke(new Action(() => FocusSelectedItem(listBox)), DispatcherPriority.Loaded);
	}

	#endregion

	#region Private Event Handlers

	private void OkButton_Click(object sender, RoutedEventArgs e)
	{
		this.DialogResult = new[] { this.fontFamilyListBox, this.fontStyleListBox, this.fontWeightListBox, this.fontSizeListBox }
			.All(listBox => listBox.SelectedItem != null);
	}

	private void FontFamilyTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		// If user enters family text, select family in list if matching item found
		this.SelectedFontFamily = new FontFamily(this.fontFamilyTextBox.Text);
	}

	private void FontStyleTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		// If user enters style text, select style in list if matching item found
		if (TryParseFontInfo<FontStyleConverter, FontStyle>(this.fontStyleTextBox.Text, out FontStyle fontStyle))
		{
			this.SelectedFontStyle = fontStyle;
		}
	}

	private void FontWeightTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		// If user enters weight text, select weight in list if matching item found
		if (TryParseFontInfo<FontWeightConverter, FontWeight>(this.fontWeightTextBox.Text, out FontWeight fontWeight))
		{
			this.SelectedFontWeight = fontWeight;
		}
	}

	private void FontSizeTextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		// If user enters size text, select size in list if matching item found.
		if (double.TryParse(this.fontSizeTextBox.Text, out double fontSize))
		{
			this.SelectedFontSize = fontSize;
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		this.Dispatcher.BeginInvoke(
			new Action(() =>
			{
				this.fontFamilyListBox.Focus();
				Keyboard.Focus(this.fontFamilyListBox);
				if (FocusSelectedItem(this.fontFamilyListBox) is ListBoxItem item)
				{
					Keyboard.Focus(item);
				}
			}),
			DispatcherPriority.Loaded);
	}

	#endregion
}
