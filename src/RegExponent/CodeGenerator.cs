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
		#region Private Data Members

		private const char Indent = '\t';

		#endregion

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

		public static string ToVerbatimLines(string value, string newline)
		{
			StringBuilder sb = new(value.Length * 2);

			string[] lines = value.Split(newline);
			foreach (string line in lines)
			{
				if (sb.Length > 0)
				{
					sb.AppendLine().Append(Indent).Append(" + ").Append(ToLiteral(newline)).Append(" + ");
				}

				sb.Append("@\"");
				foreach (char ch in line)
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
			}

			string result = sb.ToString();
			return result;
		}

		public static string GenerateBlock(Model model)
		{
			StringBuilder sb = new();

			void AppendLineTerminator()
				=> sb.AppendLine(";").AppendLine();

			void AppendConst(string name, string value)
			{
				sb.Append("const string ").Append(name).Append(" = ").Append(ToVerbatimLines(value, model.Newline));
				AppendLineTerminator();
			}

			AppendConst("Pattern", model.Pattern);

			sb.Append("Regex regex = new(Pattern, ");
			sb.Append("RegexOptions.").Append(model.Options.ToString().Replace(", ", " | RegexOptions.")).Append(')');
			AppendLineTerminator();

			AppendConst("Input", model.Input);

			switch (model.Mode)
			{
				case Mode.Match:
					sb.AppendLine("foreach (Match match in regex.Matches(Input).Cast<Match>().Where(m => m.Success))");
					sb.AppendLine("{");
					sb.Append(Indent).AppendLine("foreach (Group group in match.Groups.Cast<Group>().Skip(1).Where(g => g.Success))");
					sb.Append(Indent).AppendLine("{");
					sb.Append(Indent, 2).AppendLine("Console.WriteLine(group.Value);");
					sb.Append(Indent).AppendLine("}");
					sb.AppendLine("}");
					break;

				case Mode.Replace:
					AppendConst("Replacement", model.Replacement);
					sb.AppendLine("string replaced = regex.Replace(Input, Replacement);");
					sb.AppendLine("Console.WriteLine(replaced);");
					break;

				case Mode.Split:
					sb.AppendLine("string[] splits = regex.Split(Input);");
					sb.AppendLine("foreach (string split in splits)");
					sb.AppendLine("{");
					sb.Append(Indent).AppendLine("Console.WriteLine(split);");
					sb.AppendLine("}");
					break;
			}

			string result = sb.ToString();
			return result;
		}

		#endregion
	}
}
