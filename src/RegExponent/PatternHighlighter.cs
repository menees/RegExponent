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
		#region Constructors

		public PatternHighlighter(string text, string newline)
			: base(text, newline)
		{
		}

		#endregion

		#region Public Methods

		public override void Parse()
		{
			// TODO: Highlight regex syntax. [Bill, 4/15/2022]
			base.Parse();
		}

		#endregion
	}
}
