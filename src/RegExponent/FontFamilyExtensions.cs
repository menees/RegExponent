namespace RegExponent;

using System;
using System.Windows.Media;

internal static class FontFamilyExtensions
{
	public static bool IsMonospace(this FontFamily family)
	{
		// Use glyph advance widths — if a Typeface for the family reports equal advance
		// widths for a set of representative glyphs, treat it as monospace.
		const double epsilon = 1e-6;
		var sampleChars = new[] { 'i', 'm', 'W', '0', '1', ' ' };

		bool result = false;
		foreach (var familyTypeface in family.FamilyTypefaces)
		{
			var typeface = new Typeface(family, familyTypeface.Style, familyTypeface.Weight, familyTypeface.Stretch);
			if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyph))
			{
				continue;
			}

			double? baselineWidth = null;
			var allMatch = true;

			foreach (var ch in sampleChars)
			{
				var code = (int)ch;
				if (!glyph.CharacterToGlyphMap.TryGetValue(code, out ushort glyphIndex))
				{
					allMatch = false;
					break;
				}

				if (!glyph.AdvanceWidths.TryGetValue(glyphIndex, out double advance))
				{
					allMatch = false;
					break;
				}

				if (baselineWidth is null)
				{
					baselineWidth = advance;
				}
				else if (Math.Abs(advance - baselineWidth.Value) > epsilon)
				{
					allMatch = false;
					break;
				}
			}

			if (allMatch && baselineWidth is not null)
			{
				result = true;
				break;
			}
		}

		return result;
	}
}
