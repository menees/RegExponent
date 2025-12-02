namespace RegExponent;

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

public sealed class FontFamilyMonospaceMarkConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> value is FontFamily family && family.IsMonospace() ? "★" : string.Empty;

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}