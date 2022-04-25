namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	internal sealed class PatternHighlighter : Highlighter
	{
		#region Private Data Members

		private readonly bool ignorePatternWhitespace;

		#endregion

		#region Constructors

		public PatternHighlighter(string text, string newline, bool ignorePatternWhitespace)
			: base(text, newline)
		{
			this.ignorePatternWhitespace = ignorePatternWhitespace;
		}

		#endregion

		#region Protected Methods

		protected override void Parse()
		{
			// TODO: Highlight regex syntax. [Bill, 4/15/2022]
			// Blue: ( (? (?<name>
			// Green: \escape [range]
			// Purple: ? + *
			// Gray: #comment
			// Orange: |
			base.Parse();
			this.ignorePatternWhitespace.GetHashCode();
		}

		#endregion
	}
}
