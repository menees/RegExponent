namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;

	#endregion

	internal sealed class ReplacementHighlighter : Highlighter
	{
		#region Private Data Members

		// See https://docs.microsoft.com/en-us/dotnet/standard/base-types/substitutions-in-regular-expressions for $* syntax.
		// A group's "name must not contain any punctuation characters and cannot begin with a number." per:
		// https://docs.microsoft.com/en-us/dotnet/standard/base-types/grouping-constructs-in-regular-expressions#named-matched-subexpressions
		private static readonly Regex Substitution = new(@"\$(\d+|\{([a-zA-Z_][a-zA-Z0-9_]*?|\d+?)\}|[$&`'+_])", RegexOptions.Compiled);

		#endregion

		#region Constructors

		public ReplacementHighlighter(string text, string newline)
			: base(text, newline)
		{
		}

		#endregion

		#region Protected Methods

		protected override void Parse()
		{
			const HighlightColor PrimaryColor = HighlightColor.Blue;
			const HighlightColor AlternateColor = HighlightColor.Purple;

			MatchCollection matches = Substitution.Matches(this.Text);
			HighlightColor foreground = PrimaryColor;
			foreach (Match match in matches.Where(m => m.Success))
			{
				this.AddSegment(match.Index, match.Length, foreground: foreground);
				foreground = foreground == PrimaryColor ? AlternateColor : PrimaryColor;
			}
		}

		#endregion
	}
}
