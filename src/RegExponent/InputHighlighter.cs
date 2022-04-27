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

	internal sealed class InputHighlighter : Highlighter
	{
		#region Private Data Members

		private readonly IReadOnlyList<Match> matches;

		#endregion

		#region Constructors

		public InputHighlighter(string text, string newline, IReadOnlyList<Match> matches)
			: base(text, newline)
		{
			this.matches = matches;
		}

		#endregion

		#region Protected Methods

		protected override void Parse()
		{
			const HighlightColor PrimaryBackColor = HighlightColor.Yellow;
			const HighlightColor AlternateBackColor = HighlightColor.Orange;

			const HighlightColor PrimaryUnderColor = HighlightColor.Green;
			const HighlightColor AlternateUnderColor = HighlightColor.Red;

			HighlightColor background = PrimaryBackColor;
			foreach (Match match in this.matches.Where(m => m.Success))
			{
				this.AddSegment(match.Index, match.Length, background: background);

				HighlightColor underline = PrimaryUnderColor;
				foreach (Group group in match.Groups.Cast<Group>().Skip(1).Where(g => g.Success))
				{
					this.AddSegment(group.Index, group.Length, underline: underline);
					underline = underline == PrimaryUnderColor ? AlternateUnderColor : PrimaryUnderColor;
				}

				background = background == PrimaryBackColor ? AlternateBackColor : PrimaryBackColor;
			}
		}

		#endregion
	}
}
