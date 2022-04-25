namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	#endregion

	internal sealed class ReplacementHighlighter : Highlighter
	{
		#region Constructors

		public ReplacementHighlighter(string text, string newline)
			: base(text, newline)
		{
		}

		#endregion

		#region Public Methods

		public override void Parse()
		{
			// TODO: Highlight ${} replacers? [Bill, 4/15/2022]
			base.Parse();
		}

		#endregion
	}
}
