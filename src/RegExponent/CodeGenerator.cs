namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	internal static class CodeGenerator
	{
		#region Public Methods

		public static string ToLiteral(string value)
		{
			// https://stackoverflow.com/a/14502246/1882616
			StringBuilder sb = new(value.Length * 2);
			sb.Append('"');
			foreach (char ch in value)
			{
				switch (ch)
				{
					case '\'': sb.Append(@"\'"); break;
					case '"': sb.Append(@"\"""); break;
					case '\\': sb.Append(@"\\"); break;
					case '\0': sb.Append(@"\0"); break;
					case '\a': sb.Append(@"\a"); break;
					case '\b': sb.Append(@"\b"); break;
					case '\f': sb.Append(@"\f"); break;
					case '\n': sb.Append(@"\n"); break;
					case '\r': sb.Append(@"\r"); break;
					case '\t': sb.Append(@"\t"); break;
					case '\v': sb.Append(@"\v"); break;
					default:
						if (char.GetUnicodeCategory(ch) != UnicodeCategory.Control)
						{
							sb.Append(ch);
						}
						else
						{
							sb.Append(@"\u");
							sb.Append(((ushort)ch).ToString("x4"));
						}

						break;
				}
			}

			sb.Append('"');
			string result = sb.ToString();
			return result;
		}

		public static string ToVerbatim(string value)
		{
			StringBuilder sb = new(value.Length * 2);
			sb.Append("@\"");
			foreach (char ch in value)
			{
				switch (ch)
				{
					case '"':
						sb.Append("\"\"");
						break;
					default:
						sb.Append(ch);
						break;
				}
			}

			sb.Append('"');
			string result = sb.ToString();
			return result;
		}

		public static string GenerateBlock(Model model)
		{
			// TODO: Finish GenerateBlock. [Bill, 5/12/2022]
			return string.Empty;
		}

		#endregion
	}
}
