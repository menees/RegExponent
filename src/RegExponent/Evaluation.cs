namespace RegExponent
{
	#region Using Directives

	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	#endregion

	internal sealed class Evaluation
	{
		#region Constructors

		public Evaluation(Model model, RegexOptions options, TimeSpan timeout)
		{
			try
			{
				Regex expression = new(model.Pattern, options, timeout);
				this.Matches = expression.Matches(model.Input);

				if (model.InReplaceMode)
				{
					this.Replaced = expression.Replace(model.Input, model.Replacement);
				}
				else if (model.InSplitMode)
				{
					this.Splits = expression.Split(model.Input);
				}
			}
			catch (ArgumentException ex)
			{
				this.Exception = ex;
			}
			catch (RegexMatchTimeoutException ex)
			{
				this.Exception = ex;
			}

			this.Matches ??= Array.Empty<Match>();
			this.Replaced ??= string.Empty;
			this.Splits ??= Array.Empty<string>();
		}

		#endregion

		#region Public Properties

		public IReadOnlyList<Match> Matches { get; }

		public string Replaced { get; }

		public string[] Splits { get; }

		public Exception? Exception { get; }

		#endregion
	}
}
