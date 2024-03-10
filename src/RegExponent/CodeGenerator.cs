namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Menees;

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

		public static string ToRawLines(string value, string newline)
		{
			StringBuilder sb = new(value.Length * 2);

			// Use C# 11's raw string literals since their design motivation was to make embedding regexes easier.
			// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-11.0/raw-string-literal#motivation
			const int MinQuotes = 3;
			const int MaxQuotes = 100;
			sb.Append('"', MinQuotes);
			while (sb.Length <= MaxQuotes)
			{
				if (!value.Contains(sb.ToString()))
				{
					break;
				}

				sb.Append('"');
			}

			if (sb.Length >= MaxQuotes)
			{
				throw Exceptions.NewArgumentException("Input value contains too long of a sequence of double quotes to generate readable code.");
			}

			string delimiter = sb.ToString();
			sb.Clear();
			string[] lines = value.Split(newline);
			foreach (string line in lines)
			{
				if (sb.Length > 0)
				{
					// Since Git changes newlines by default to match the OS, we need to encode
					// the exact newline the model requires for this pattern.
					sb.AppendLine().Append(Indent).Append("+ ").Append(ToLiteral(newline)).Append(" + ");
				}

				sb.Append(delimiter);
				sb.Append(line);
				sb.Append(delimiter);
			}

			string result = sb.ToString();
			return result;
		}

		public static string ToGeneratedRegex(Model model, string methodName)
		{
			StringBuilder sb = new();
			sb.Append("[GeneratedRegex(");
			sb.Append(ToRawLines(model.Pattern, model.Newline));

			if (model.Options != RegexOptions.None)
			{
				sb.Append(", ");
				AppendOptions(sb, model);
			}

			sb.Append(")]");
			sb.AppendLine();
			sb.Append("private static partial Regex ");
			sb.Append(methodName);
			sb.Append("();");

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
				sb.Append("const string ").Append(name).Append(" = ").Append(ToRawLines(value, model.Newline));
				AppendLineTerminator();
			}

			AppendConst("Pattern", model.Pattern);

			sb.Append("Regex regex = new(Pattern, ");
			AppendOptions(sb, model);
			sb.Append(')');
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

		#region Private Methods

		private static void AppendOptions(StringBuilder sb, Model model)
			=> sb.Append("RegexOptions.").Append(model.Options.ToString().Replace(", ", " | RegexOptions."));

		#endregion
	}
}
